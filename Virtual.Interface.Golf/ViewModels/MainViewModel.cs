using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Virtual.Interface.Golf.Models;
using Virtual.Interface.Golf.DataAccess;
using System.Collections.ObjectModel;
using Virtual.Interface.Golf.Sockets;
using System.Configuration;
using System.Windows.Threading;
using System.Windows.Input;

using Virtual.Interface.Golf.Commands;
using Virtual.Interface.Golf.Output; 

namespace Virtual.Interface.Golf.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Private Members

        private Talker _talker;

        private Tournament _tournament;
        private List<Round> _rounds;
        private Round _selectedRound;

        private ObservableCollection<Pairing> _pairings;
        private Pairing _selectedPairing;

        private ObservableCollection<Player> _players;
        private Player _selectedPlayer;

        private ObservableCollection<ShotItem> _shotItems;
        private ShotItem _selectedShotItem;
        private ObservableCollection<ShotItem> _itemsOnPreview;
        private ShotItem _selectedItemOnPreview;
        private ObservableCollection<ShotItem> _itemsOnAir;
        private ShotItem _selectedItemOnAir;

        private List<Hole> _holes;
        private Hole _selectedHole; 

        private string _hitLocation;
        private string _engineConnectionStatus;

        private DispatcherTimer _reconnectEngineTimer;

        private bool _engineConnectionAlive = false;

        private DelegateCommand _initVGACommand;
        private DelegateCommand _sendToPreviewCommand;
        private DelegateCommand _sendToAirCommand;
        private DelegateCommand _refreshShotItemsCommand;
        private DelegateCommand _removeFromPreviewCommand; 

        #endregion

        #region Properties

        public Tournament Tournament
        {
            get { return _tournament; }
            set { _tournament = value; OnPropertyChanged("Tournament"); }
        }

        public List<Round> Rounds
        {
            get { return _rounds; }
            set { _rounds = value; OnPropertyChanged("Rounds"); }
        }

        public Round SelectedRound
        {
            get { return _selectedRound; }
            set
            {
                _selectedRound = value;
                OnPropertyChanged("SelectedRound");

                loadPairings();
            }
        }

        public List<Hole> Holes
        {
            get { return _holes; }
            set { _holes = value; OnPropertyChanged("Holes"); }
        }

        public Hole SelectedHole
        {
            get { return _selectedHole; }
            set 
            { 
                _selectedHole = value; 
                OnPropertyChanged("SelectedHole");

                loadExtraItems();

                //send engine file new calibration & telemetry files
                sendCalibrationFileToEngine();
                sendTelemetryFileToEngine();
            }
        }

        public ObservableCollection<Player> Players
        {
            get { return _players; }
            set { _players = value; OnPropertyChanged("Players"); }
        }

        public Player SelectedPlayer
        {
            get { return _selectedPlayer; }
            set 
            { 
                _selectedPlayer = value; 
                OnPropertyChanged("SelectedPlayer"); 
            
                //load this player's previous shots into the shot items list
                loadPlayerPreviousShots();

                //send message to engine to track selected player

            }
        }

        public ObservableCollection<Pairing> Pairings
        {
            get { return _pairings; }
            set { _pairings = value; OnPropertyChanged("Pairings"); }
        }

        public Pairing SelectedPairing
        {
            get { return _selectedPairing; }
            set { _selectedPairing = value; OnPropertyChanged("SelectedPairing"); }
        }

        public ObservableCollection<ShotItem> ShotItems
        {
            get { return _shotItems; }
            set { _shotItems = value; OnPropertyChanged("ShotItems"); }
        }

        public ShotItem SelectedShotItem
        {
            get { return _selectedShotItem; }
            set { _selectedShotItem = value; OnPropertyChanged("SelectedShotItem"); }
        }

        public ObservableCollection<ShotItem> ItemsOnPreview
        {
            get { return _itemsOnPreview; }
            set { _itemsOnPreview = value; OnPropertyChanged("ItemsOnPreview"); }
        }

        public ShotItem SelectedItemOnPreview
        {
            get { return _selectedItemOnPreview; }
            set { _selectedItemOnPreview = value; OnPropertyChanged("SelectedItemOnPreview"); }
        }

        public ObservableCollection<ShotItem> ItemsOnAir
        {
            get { return _itemsOnAir; }
            set { _itemsOnAir = value; OnPropertyChanged("ItemsOnAir"); }
        }

        public ShotItem SelectedItemOnAir
        {
            get { return _selectedItemOnAir; }
            set { _selectedItemOnAir = value; OnPropertyChanged("SelectedItemOnAir"); }
        }

        public string HitLocation
        {
            get { return _hitLocation; }
            set 
            { 
                _hitLocation = value; 
                OnPropertyChanged("HitLocation"); 
            
                //add the current player's shot to the "on preview" list

                //remove any current player's shot from preview?
            }
        }

        public string EngineConnectionStatus
        {
            get { return _engineConnectionStatus; }
            set { _engineConnectionStatus = value; OnPropertyChanged("EngineConnectionStatus"); }
        }

        #endregion

        #region Constructor

        public MainViewModel()
        {
            _reconnectEngineTimer = new DispatcherTimer();
            _reconnectEngineTimer.Tick += new EventHandler(reconnectEngineTimerElapsed);
            _reconnectEngineTimer.Interval = new TimeSpan(0, 0, 1);
            _reconnectEngineTimer.IsEnabled = false;

            initializeTalker();

            _tournament = DbConnection.GetTournament();

            _rounds = DbConnection.GetRounds();
            _selectedRound = _rounds[0];

            _holes = _tournament.Event.Courses[0].Holes.Where(h => h.HoleNum == 5 || h.HoleNum == 14 || h.HoleNum == 17).ToList();

            loadPlayers();            

            loadPairings();

            loadExtraItems();

            _itemsOnPreview = new ObservableCollection<ShotItem>();
            _itemsOnAir = new ObservableCollection<ShotItem>();
        }

        #endregion

        #region Private Methods

        private void loadPlayers()
        {
            _players = DbConnection.GetPlayers();
        }

        private void loadPairings()
        {
            ObservableCollection<Pairing> pairings = DbConnection.GetPairings(_players.ToList(), _selectedRound.RoundID);

            Pairings = pairings;            
        }

        private void loadExtraItems()
        {
            if (_selectedHole != null && _players != null)
            {
                ObservableCollection<ShotItem> items = new ObservableCollection<ShotItem>();

                ShotItem item = null;

                //birdies
                item = new ShotItem();
                List<Shot> birdies = DbConnection.GetShots(_selectedHole.HoleID, 1, -1, _players.ToList());
                item.Description = "All Birdies";
                item.Type = "BIRDIES";
                item.Shots = birdies;
                items.Add(item);

                //pars
                item = new ShotItem();
                List<Shot> pars = DbConnection.GetShots(_selectedHole.HoleID, 1, 0, _players.ToList());
                item.Description = "All Pars";
                item.Type = "PARS";
                item.Shots = pars;
                items.Add(item);

                //bogeys
                item = new ShotItem();
                List<Shot> bogeys = DbConnection.GetShots(_selectedHole.HoleID, 1, 1, _players.ToList());
                item.Description = "All Bogeys";
                item.Type = "BOGEYS";
                item.Shots = bogeys;
                items.Add(item);

                //double bogeys or worse
                item = new ShotItem();
                List<Shot> dblbogeys = DbConnection.GetShots(_selectedHole.HoleID, 1, 2, _players.ToList());
                item.Description = "All Double Bogeys or Worse";
                item.Type = "DBLBOGEYS";
                item.Shots = dblbogeys;
                items.Add(item);

                //longest drive(s)
            
                item = new ShotItem();
                List<Shot> longestDrive = DbConnection.GetLongestDrive(_selectedHole.HoleID, 1, _players.ToList());

                string desc = "Longest Drive Overall";

                if (longestDrive.Count > 0)
                {
                    desc += " (";

                    foreach (Shot shot in longestDrive)
                    {
                        desc += shot.Player.LastName + ", ";
                    }

                    desc = desc.Substring(0, desc.Length - 2);

                    desc += " - " + longestDrive[0].VirtualDistance + " yds)";
                }
                else
                {
                    desc += " (none)";
                }

                item.Description = desc;
                item.Type = "LONGESTDRIVE";
                item.Shots = longestDrive;
                items.Add(item);

                for (int i = 1; i <= 4; i++)
                {
                    item = new ShotItem();
                    longestDrive = DbConnection.GetLongestDrive(_selectedHole.HoleID, 1, _players.ToList(), i);
                    

                    desc = "Longest Drive Round " + i.ToString();

                    if (longestDrive.Count > 0)
                    {
                        desc += " (";

                        foreach (Shot shot in longestDrive)
                        {
                            desc += shot.Player.LastName + ", ";
                        }

                        desc = desc.Substring(0, desc.Length - 2);

                        desc += " - " + longestDrive[0].VirtualDistance + " yds)";
                    }
                    else
                    {
                        desc += " (none)";
                    }

                    item.Description = desc;
                    item.Type = "LONGESTDRIVE";
                    item.Shots = longestDrive;
                    items.Add(item);
                }
            

                ShotItems = items;
            }
        }

        private void loadPlayerPreviousShots()
        {
            for(int i = _shotItems.Count - 1; i >= 0; i--) 
            {
                if(_shotItems[i].Type == "PLAYERSHOT") _shotItems.RemoveAt(i);
            }

            if (_selectedPlayer != null && _selectedHole != null)
            {
                List<Shot> shots = DbConnection.GetShots(_selectedHole.HoleID, 1, _selectedPlayer.PlayerID.ToString(), _players.ToList());

                foreach (Shot shot in shots)
                {
                    if (shot.Round.RoundID != _selectedRound.RoundID) //don't put the current round's shot up there.  it may not have been saved yet, and it will be put on preview by clicking on live preview
                    {
                        ShotItem shotItem = new ShotItem();
                        shotItem.Type = "PLAYERSHOT";
                        shotItem.Shots = new List<Shot>();
                        shotItem.Shots.Add(shot);

                        string distance = "";

                        if (shot.VirtualDistance.Trim() != "")
                        {
                            distance = "(" + shot.VirtualDistance + " yds)";
                        }

                        shotItem.Description = _selectedPlayer.LastName + " - Round " + shot.Round.RoundNum.ToString() + " drive " + distance;

                        ShotItems.Add(shotItem);
                    }
                }
            }
        }

        private void sendToPreview()
        {
            if (_selectedShotItem != null)
            {
                if (_itemsOnPreview != null)
                {
                    SocketCommand commandToSend = new SocketCommand();

                    XmlDataRow xmlRow = new XmlDataRow();

                    //send the item to the preview scene, will need to add instancing so more than one item can be put on each layer
                    switch (_selectedShotItem.Type)
                    {
                        case "LONGESTDRIVE":
                        case "PLAYERSHOT":
                            _selectedShotItem.Template = "DistanceMarker";

                            xmlRow.Add("Marker_Translate", _selectedShotItem.Shots[0].VirtualLocation.ToString());
                            xmlRow.Add("Distance", _selectedShotItem.Shots[0].VirtualDistance.ToString());
                            xmlRow.Add("Name", _selectedShotItem.Shots[0].Player.LastName);
                            xmlRow.Add("Headshot", _selectedShotItem.Shots[0].Player.Headshot.AbsolutePath);

                            _selectedShotItem.Shots[0].TemplateData = xmlRow.ToString();

                            break;

                        case "BIRDIES":
                        case "PARS":
                        case "BOGEYS":
                        case "DBLBOGEYS":
                            _selectedShotItem.Template = "ScoreMarker";

                            foreach (Shot shot in _selectedShotItem.Shots)
                            {
                                xmlRow.Add("Marker_Translate", shot.VirtualLocation.ToString());
                                shot.TemplateData += xmlRow.ToString();
                            }

                            break;
                    }

                    commandToSend.Command = CommandType.ShowPage;
                    commandToSend.CommandID = Guid.NewGuid().ToString();
                    commandToSend.Parameters = new List<CommandParameter>();
                    commandToSend.Parameters.Add(new CommandParameter("TemplateName", _selectedShotItem.Template));
                    commandToSend.Parameters.Add(new CommandParameter("DestScene", "Preview"));
                    commandToSend.TemplateData = xmlRow.GetXMLString();
                            
                    _talker.Talk(commandToSend);

                    //update the items on preview list (may have to wait for an acknowledgement from the scene first...
                    ShotItem _previewItem = _itemsOnPreview.SingleOrDefault(i => i.Description == _selectedShotItem.Description);

                    if (_previewItem == null)
                    {
                        ItemsOnPreview.Add(_selectedShotItem);
                    }
                }
                else
                {
                    ItemsOnPreview.Add(_selectedShotItem);
                }
            }
        }

        private void removeFromPreview()
        {
            //remove the selected item from the preview scene

            //remove the selected item from the list
            if (_selectedItemOnPreview != null)
            {
                _itemsOnPreview.Remove(_selectedItemOnPreview);
            }
        }

        private void sendToAir()
        {
            if (_selectedItemOnPreview != null)
            {
                if (_itemsOnAir != null)
                {
                    SocketCommand commandToSend = new SocketCommand();

                    XmlDataRow xmlRow = new XmlDataRow();

                    //send the item to the output scene, will need to add instancing so more than one item can be put on each layer
                    foreach (ShotItem shotItem in _itemsOnPreview)
                    {
                        string templateData = "";

                        foreach (Shot shot in shotItem.Shots)
                        {
                            templateData += shot.TemplateData;
                        }

                        commandToSend.Command = CommandType.ShowPage;
                        commandToSend.CommandID = Guid.NewGuid().ToString();
                        commandToSend.Parameters = new List<CommandParameter>();
                        commandToSend.Parameters.Add(new CommandParameter("TemplateName", _selectedShotItem.Template));
                        commandToSend.Parameters.Add(new CommandParameter("DestScene", "AIR"));
                        commandToSend.TemplateData = templateData;

                        _talker.Talk(commandToSend);
                    }

                    

                    //update the items on the air list 
                    ShotItem _airItem = _itemsOnAir.SingleOrDefault(i => i.Description == _selectedItemOnPreview.Description);

                    if (_airItem == null)
                    {
                        ItemsOnAir.Add(_selectedItemOnPreview);
                    }
                }
                else
                {
                    ItemsOnAir.Add(_selectedItemOnPreview);
                }
            }
        }

        private void reconnectEngineTimerElapsed(object sender, EventArgs e)
        {
            _reconnectEngineTimer.IsEnabled = false;

            initializeTalker();
        }

        private void initVGA()
        {
            SocketCommand commandToSend = new SocketCommand();

            commandToSend.ID = Guid.NewGuid().ToString();
            commandToSend.Command = CommandType.Initialize;

            commandToSend.Parameters = new List<CommandParameter>();
            commandToSend.Parameters.Add(new CommandParameter("DisplayType", 0));
            commandToSend.Parameters.Add(new CommandParameter("TemplateDirectory", ConfigurationManager.AppSettings["TemplateDirectory"].ToString()));
            commandToSend.Parameters.Add(new CommandParameter("WorkingDirectory", ConfigurationManager.AppSettings["TemplateDirectory"].ToString()));
            commandToSend.Parameters.Add(new CommandParameter("FormatType", 2));

            _talker.Talk(commandToSend);
        }

        private void sendCalibrationFileToEngine()
        {
            SocketCommand commandToSend = new SocketCommand();

            commandToSend.ID = Guid.NewGuid().ToString();
            commandToSend.Command = CommandType.SetCalibrationFile;

            commandToSend.Parameters = new List<CommandParameter>();
            commandToSend.Parameters.Add(new CommandParameter("CalibrationFile", ConfigurationManager.AppSettings["CalibrationFile_" + _selectedHole.HoleNum.ToString()]));

            _talker.Talk(commandToSend);
        }

        private void sendTelemetryFileToEngine()
        {
            SocketCommand commandToSend = new SocketCommand();

            commandToSend.ID = Guid.NewGuid().ToString();
            commandToSend.Command = CommandType.SetTelemetryFile;

            commandToSend.Parameters = new List<CommandParameter>();
            commandToSend.Parameters.Add(new CommandParameter("TelemetryFile", ConfigurationManager.AppSettings["TelemetryFile_" + _selectedHole.HoleNum.ToString()]));

            _talker.Talk(commandToSend);
        }

        #endregion

        #region Talker

        private void initializeTalker()
        {
            if (_talker != null)
            {
                _talker.DataArrival -= talkerDataArrival;
                _talker.Connected -= talkerConnected;
                _talker.ConnectionClosed -= talkerConnectionClosed;
                _talker.Dispose();
                _talker = null;
            }

            _talker = new Talker("1");
            _talker.DataArrival += new Talker.DataArrivalHandler(talkerDataArrival);
            _talker.Connected += new Talker.ConnectionHandler(talkerConnected);
            _talker.ConnectionClosed += new Talker.ConnectionClosedHandler(talkerConnectionClosed);
            _talker.ConnectionTimeout += new Talker.ConnectionTimeoutHandler(talkerConnectionTimeout);

            _talker.Connect(ConfigurationManager.AppSettings["EngineIp"].ToString(), ConfigurationManager.AppSettings["EnginePort"].ToString());
        }

        private void talkerConnected()
        {
            _engineConnectionAlive = true;
            _reconnectEngineTimer.IsEnabled = false;

            EngineConnectionStatus = "Connected to Virtual Engine (" + ConfigurationManager.AppSettings["EngineIp"].ToString() + ":" + ConfigurationManager.AppSettings["EnginePort"].ToString() + ")";
        }

        private void talkerDataArrival(SocketCommand CommandToProcess, string ID)
        {
            //KeyValuePair<string, int>? playlistCommand = null;

            switch (CommandToProcess.Command.ToString())
            {
                case "CommandSuccessful":

                    break;
                case "CommandFailed":

                    break;
                case "HitLocation":
                    HitLocation = getCommandParameter("Location", CommandToProcess).ToString();
                    break;
            }
        }

        private void talkerConnectionClosed()
        {
            EngineConnectionStatus = "Not Connected to Virtual Engine";

            _engineConnectionAlive = false;
            _reconnectEngineTimer.IsEnabled = true;
        }

        private void talkerConnectionRefused()
        {
            EngineConnectionStatus = "Not Connected to Virtual Engine (connection refused)";

            _engineConnectionAlive = false;
            _reconnectEngineTimer.IsEnabled = true;
        }

        private void talkerConnectionTimeout()
        {
            EngineConnectionStatus = "Connection to Virtual Engine timed out";

            _engineConnectionAlive = false;
            _reconnectEngineTimer.IsEnabled = true;
        }

        private object getCommandParameter(string parameterName, SocketCommand commandToSearch)
        {
            try
            {
                CommandParameter outputFormatParameter = commandToSearch.Parameters.Find(delegate(CommandParameter foundList) { return foundList.Name == parameterName; });
                if (outputFormatParameter != null) { return outputFormatParameter.Value; }

            }
            catch (Exception ex)
            {
                return null;
                //throw;
            }

            return null;
        }

        #endregion

        #region Commands

        public ICommand InitVGACommand
        {
            get
            {
                if (_initVGACommand == null)
                {
                    _initVGACommand = new DelegateCommand(initVGA);
                }
                return _initVGACommand;
            }
        }

        public ICommand SendToPreviewCommand
        {
            get
            {
                if (_sendToPreviewCommand == null)
                {
                    _sendToPreviewCommand = new DelegateCommand(sendToPreview);
                }
                return _sendToPreviewCommand;
            }
        }

        public ICommand RemoveFromPreviewCommand
        {
            get
            {
                if (_removeFromPreviewCommand == null)
                {
                    _removeFromPreviewCommand = new DelegateCommand(removeFromPreview);
                }
                return _removeFromPreviewCommand;
            }
        }

        public ICommand SendToAirCommand
        {
            get
            {
                if (_sendToAirCommand == null)
                {
                    _sendToAirCommand = new DelegateCommand(sendToAir);
                }
                return _sendToAirCommand;
            }
        }

        public ICommand RefreshShotItemsCommand
        {
            get
            {
                if (_refreshShotItemsCommand == null)
                {
                    _refreshShotItemsCommand = new DelegateCommand(loadExtraItems);
                }
                return _refreshShotItemsCommand;
            }
        }
        
        
        #endregion
    }
}
