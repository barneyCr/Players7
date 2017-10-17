namespace Players7Server.Enums
{
	public enum AuthMethod
	{
		UsernameOnly,
		Full,
		InviteCode
	}

	enum LogMessageType
	{
		Config,
		Network,
		Chat,
		Auth,
		UserEvent,
		Packet,
		ReportFromUser,
		Error,
		Warning,
		OK,
		ServerMessage
	}

	static class HeaderTypes
	{
		public const int INIT_CLIENT = 1;
		public const int KICK = -1;
		public const int BROADCAST = 3;
		public const int SYSTEM_MESSAGE = 4;
		public const int GET_ROLE_OF = 5;
		public const int CLIENT_DISCONNECTED = 12;
		public const int ADD_PREV_USER = 29;
		public const int ADD_NEW_USER = 31;
		public const int POST_MESSAGE = 32;
		public const int NOTIFY_POST = 34;
		public const int SEND_WHISPER = 35;
		public const int RECEIVE_WHISPER = 37;
		public const int SENT_WHISPER = 38;
		public const int WHISPER_ERROR = -38;
		public const int CHANGE_USERNAME_REQUEST = 41;
		public const int CHANGE_USERNAME_DENIED = -41;
		public const int CHANGE_USERNAME_ANNOUNCE = 42;
		public const int REPORT_USER = 43;
		public const int REQUEST_SEND_FILE = 100;

        public const int GAME_CREATE = 101;  // 101|3|1|joc 1
		public const int GAME_ADD_GENERAL_INFO = 102;
		public const int GAME_REMOVE = -102;
        public const int GAME_JOIN_REQUEST = 103;
        public const int GAME_JOIN_REQUEST_ERROR = -103;
        public const int GAME_INSIDE_INFO = 104;
        public const int GAME_INIT = 105;
        public const int GAME_TURN_OF = 106;
        public const int GAME_PLAYER_PUT_CARD = 107;
        public const int GAME_PLAYER_PUT_CARD_ERROR = -107;
        public const int GAME_PACK_UPDATE_PutONTABLE = 108;
        public const int GAME_PACK_UPDATE_SELF = 109;
        public const int GAME_PACK_SHUFFLED = 110;
        public const int GAME_PLAYERS_ALL_ADDED = 111;
        public const int GAME_PLAYER_READY = 112;
        public const int GAME_CARDS_FLOAT_SET = 113;
        public const int GAME_PLAYER_FINISHED_PLACE = 114;
	}
}