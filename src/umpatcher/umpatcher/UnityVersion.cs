/*
    Copyright (C) 2018 de4dot@gmail.com

    This file is part of umpatcher

    umpatcher is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    umpatcher is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with umpatcher.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Text.RegularExpressions;

namespace UnityMonoDllSourceCodePatcher {
	readonly struct UnityVersionReleaseType : IComparable<UnityVersionReleaseType> {
		public readonly string Type;

		private UnityVersionReleaseType(string type) => Type = type;

		public static bool TryParse(string value, out UnityVersionReleaseType releaseType) {
			releaseType = default;
			if (!(value == "a" || value == "b" || value == "rc"
				|| value == "" || value == "f" || value == "c"
				|| value == "p" || value == "x"))
				return false;
			releaseType = new UnityVersionReleaseType(value);
			return true;
		}

		public static UnityVersionReleaseType Alpha = new UnityVersionReleaseType("a");
		public static UnityVersionReleaseType Beta = new UnityVersionReleaseType("b");
		public static UnityVersionReleaseType ReleaseCandidate = new UnityVersionReleaseType("rc");
		public static UnityVersionReleaseType Untyped = new UnityVersionReleaseType("");
		public static UnityVersionReleaseType Final = new UnityVersionReleaseType("f");
		public static UnityVersionReleaseType Chinese = new UnityVersionReleaseType("c");
		public static UnityVersionReleaseType Patch = new UnityVersionReleaseType("p");
		public static UnityVersionReleaseType Experimental = new UnityVersionReleaseType("x");

		public byte ToNumber() =>
			Type == "a" ? (byte)0 :
			Type == "b" ? (byte)1 :
			Type == "rc" ? (byte)2 :
			Type == "" ? (byte)3 :
			Type == "f" || Type == "c" ? (byte)4 :
			Type == "p" ? (byte)5 :
			Type == "x" ? (byte)6 :
			throw new InvalidOperationException($"Unknown Unity release type: {Type}");

		public int CompareTo(UnityVersionReleaseType other) => ToNumber().CompareTo(other.ToNumber());

		public override string ToString() =>
			Type == "a" ? nameof(Alpha) :
			Type == "b" ? nameof(Beta) :
			Type == "rc" ? nameof(ReleaseCandidate) :
			Type == "" ? nameof(Untyped) :
			Type == "f" ? nameof(Final) :
			Type == "c" ? nameof(Chinese) :
			Type == "p" ? nameof(Patch) :
			Type == "x" ? nameof(Experimental) :
			throw new InvalidOperationException($"Unknown Unity release type: {Type}");
	}
	readonly struct UnityVersionRelease : IComparable<UnityVersionRelease> {
		public readonly UnityVersionReleaseType ReleaseType;
		public readonly uint Release;

		public UnityVersionRelease(UnityVersionReleaseType releaseType, uint release) {
			ReleaseType = releaseType;
			Release = release;
		}

		public static bool TryParse(string value, out UnityVersionRelease release) {
			release = default;
			if (value == "") {
				release = UnityVersionRelease.Untyped;
				return true;
			}
			var m = Regex.Match(value, @"^(a|b|rc|f|p|x)(\d+)$");
			if (!m.Success)
				return false;
			if (m.Groups.Count != 3)
				return false;
			if (!UnityVersionReleaseType.TryParse(m.Groups[1].Value, out UnityVersionReleaseType releaseType))
				return false;
			if (!uint.TryParse(m.Groups[2].Value, out uint _release))
				return false;
			release = new UnityVersionRelease(releaseType, _release);
			return true;
		}

		public static UnityVersionRelease Untyped = new UnityVersionRelease(UnityVersionReleaseType.Untyped, 0);

		public int CompareTo(UnityVersionRelease other) {
			int c = ReleaseType.CompareTo(other.ReleaseType);
			if (c != 0)
				return c;
			return ReleaseType.CompareTo(UnityVersionReleaseType.Untyped) == 0 ? 0 : Release.CompareTo(other.Release);
		}
		public override string ToString() =>
			ReleaseType.CompareTo(UnityVersionReleaseType.Untyped) == 0 ? "" : $"{ReleaseType.Type}{Release}";
	}

	readonly struct UnityVersion : IComparable<UnityVersion> {
		public readonly uint Major;
		public readonly uint Minor;
		public readonly uint Revision;
		public readonly UnityVersionRelease Release;
		public readonly string Suffix;

		public UnityVersion(uint major, uint minor, uint revision, UnityVersionRelease release, string suffix) {
			Major = major;
			Minor = minor;
			Revision = revision;
			Release = release;
			Suffix = suffix;
		}

		public static bool TryParse(string value, out UnityVersion version) {
			version = default;
			if (value == "")
				return false;
			var m = Regex.Match(value, @"^(\d+)\.(\d+)\.(\d+)((a|b|rc|f|p|x)\d+)?(-[\-.\w]*)?$");
			if (!m.Success)
				return false;
			if (m.Groups.Count != 7)
				return false;
			if (!uint.TryParse(m.Groups[1].Value, out uint major))
				return false;
			if (!uint.TryParse(m.Groups[2].Value, out uint minor))
				return false;
			if (!uint.TryParse(m.Groups[3].Value, out uint revision))
				return false;
			if (!UnityVersionRelease.TryParse(m.Groups[4].Value, out UnityVersionRelease release))
				return false;

			version = new UnityVersion(major, minor, revision, release, m.Groups[6].Value);
			return true;
		}

		public int CompareTo(UnityVersion other) {
			int c = Major.CompareTo(other.Major);
			if (c != 0)
				return c;
			c = Minor.CompareTo(other.Minor);
			if (c != 0)
				return c;
			c = Revision.CompareTo(other.Revision);
			if (c != 0)
				return c;
			c = Release.CompareTo(other.Release);
			if (c != 0)
				return c;
			return StringComparer.OrdinalIgnoreCase.Compare(Suffix, other.Suffix);
		}

		public override string ToString() => $"{Major}.{Minor}.{Revision}{Release}{Suffix}";
	}
}
