using Tarabish.Extensions;
using UnityEngine;

namespace Tarabish {
	public class ProjectManager : Singleton<ProjectManager> {
	    [System.Flags] public enum LogLevels {
			INFO = 			1 << 1,
			MESSAGE = 		1 << 2,
			NOTIFY = 		1 << 3,
			ANNOUNCEMENT = 	1 << 4,
			DEBUG = 		1 << 5,
			VERBOSE = 		1 << 6,
		}

		public static LogLevels LogLevel;
		[SerializeField] private LogLevels _logLevel;
		
		public static bool DevBuild = true;
		[SerializeField] private bool _devBuild = true;

		public static bool RELEASE = true; //? Is this a release version
		[SerializeField] private bool _release = true;

		protected override void Awake() {
			base.Awake();
			OnValidate();

			this.LogDebug($"DevBuild: {DevBuild} | RELEASE: {RELEASE} | IsEditor: {Application.isEditor}");
		}

		private void OnValidate() {
			LogLevel = _logLevel;

			DevBuild = _devBuild;
			RELEASE = _release;
		}
	}
}
