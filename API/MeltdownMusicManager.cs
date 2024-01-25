using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FacilityMeltdown.API {
    public static class MeltdownMusicManager {
        public readonly static Func<SelectableLevel, AudioClip> MusicResolver = (__) => { return null; };

        public static AudioClip GetMusic(SelectableLevel level) {
            List<AudioClip> resolvedMusic = new List<AudioClip>();

            foreach(Func<SelectableLevel, AudioClip> resolver in MusicResolver.GetInvocationList()) {
                AudioClip resolved = resolver(level);
                if(resolved != null) {
                    resolvedMusic.Add(resolved);
                }
            }

            if(resolvedMusic.Count == 0) {
                return Assets.defaultMusic;
            }
            return resolvedMusic[UnityEngine.Random.Range(0, resolvedMusic.Count)];
        }
    }
}
