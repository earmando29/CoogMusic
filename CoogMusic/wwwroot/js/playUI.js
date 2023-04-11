let queue = [];
let currentSongIndex = 0;

function playSongFromSearch(songId, songTitle, artistName) {
    const song = {
        songId: songId,
        title: songTitle,
        artistName: artistName
    };
    queue = [song]; // Set queue to contain only the current song
    currentSongIndex = 0;
    playSong(song);
}

function playSongFromPlaylist(playlistSongs, index) {
    queue = playlistSongs; // Update the queue with the entire playlist
    currentSongIndex = index;
    playSong(queue[currentSongIndex]);
}

function playSong(song) {
    if (!song) return;
    //console.log(song);

    // Make an AJAX request to get the song data
    document.getElementById("song-title").innerText = capitalizeFirstLetter(song.title.toLowerCase());
    document.getElementById("artist-name").innerText = capitalizeFirstLetter(song.artistName.toLowerCase());
    var xhr = new XMLHttpRequest();
    xhr.open('GET', '/Search/Index?handler=PlaySong&id=' + song.songId, true);
    xhr.responseType = 'blob';
    xhr.onload = function (e) {
        if (this.status == 200) {
            // Create a blob URL for the audio data
            var blob = new Blob([this.response], { type: 'audio/mpeg' });
            var url = URL.createObjectURL(blob);

            // Update the audio player to play the new audio file
            var audio = document.getElementById('audio-player');
            audio.src = url;
            audio.play();

            // Update the play-pause button icon to the pause icon
            const playPauseButton = document.getElementById('play-pause-button');
            const playPauseIcon = playPauseButton.getElementsByTagName('i')[0];
            playPauseButton.innerHTML = '<i class="fas fa-pause"></i>';
        }
    };
    xhr.send();
}

function playNext() {
    if (currentSongIndex < queue.length - 1) {
        currentSongIndex++;
        playSong(queue[currentSongIndex]);
    }
}

function playPrevious() {
    if (currentSongIndex > 0) {
        currentSongIndex--;
        playSong(queue[currentSongIndex]);
    }
}

function capitalizeFirstLetter(string) {
    return string.split(' ').map(function (word) {
        return word.charAt(0).toUpperCase() + word.slice(1);
    }).join(' ');
}

document.addEventListener("DOMContentLoaded", function () {
    // Get UI elements
    const audioPlayer = document.getElementById("audio-player");
    const playPauseButton = document.getElementById("play-pause-button");
    const volumeSlider = document.getElementById("volume-slider");
    const currentTime = document.getElementById("current-time");
    const fullLength = document.getElementById("full-length");
    const musicProgress = document.getElementById("music-progress");

    const skipButton = document.getElementById("skip-button");
    const prevButton = document.getElementById("prev-button");

    skipButton.addEventListener("click", function () {
        playNext();
    });

    prevButton.addEventListener("click", function () {
        playPrevious();
    });

    // Play/pause functionality
    playPauseButton.addEventListener("click", function () {
        if (audioPlayer.paused) {
            audioPlayer.play();
            playPauseButton.innerHTML = '<i class="fas fa-pause"></i>';
        } else {
            audioPlayer.pause();
            playPauseButton.innerHTML = '<i class="fas fa-play"></i>';
        }
    });

    // Update volume
    volumeSlider.addEventListener("input", function () {
        audioPlayer.volume = volumeSlider.value;
    });

    // Update current time and progress bar
    audioPlayer.addEventListener("timeupdate", function () {
        currentTime.textContent = formatTime(audioPlayer.currentTime);
        fullLength.textContent = formatTime(audioPlayer.duration);
        if (isFinite(audioPlayer.currentTime) && isFinite(audioPlayer.duration))
            musicProgress.value = (audioPlayer.currentTime / audioPlayer.duration) * 100;
    });

    // Seek functionality
    musicProgress.addEventListener("input", function () {
        audioPlayer.currentTime = (musicProgress.value / 100) * audioPlayer.duration;
    });

    // Format time in minutes and seconds (MM:SS)
    function formatTime(seconds) {
        const minutes = Math.floor(seconds / 60);
        const remainingSeconds = Math.floor(seconds % 60);
        return minutes.toString().padStart(2, "0") + ":" + remainingSeconds.toString().padStart(2, "0");
    }
});
