const btnTweets = document.getElementById("btnTweets");
const btnReTweets = document.getElementById("btnReTweets");
const btnLikes = document.getElementById("btnLikes");
const panelTweets = document.getElementById("panelTweets");
const panelReTweets = document.getElementById("panelReTweets");
const panelLikes = document.getElementById("panelLikes");

btnTweets.addEventListener("click", (e) => {
    if (panelTweets.classList.contains("d-none")) {
        panelTweets.classList.remove("d-none");
        panelReTweets.classList.add("d-none");
        panelLikes.classList.add("d-none");
    }
    if (!e.currentTarget.classList.contains("active")) {
        e.currentTarget.classList.add("active");
        btnReTweets.classList.remove("active");
        btnLikes.classList.remove("active");
    }
});

btnReTweets.addEventListener("click", (e) => {
    if (panelReTweets.classList.contains("d-none")) {
        panelReTweets.classList.remove("d-none");
        panelTweets.classList.add("d-none");
        panelLikes.classList.add("d-none");
    }
    if (!e.currentTarget.classList.contains("active")) {
        e.currentTarget.classList.add("active");
        btnTweets.classList.remove("active");
        btnLikes.classList.remove("active");
    }
});

btnLikes.addEventListener("click", (e) => {
    if (panelLikes.classList.contains("d-none")) {
        panelLikes.classList.remove("d-none");
        panelTweets.classList.add("d-none");
        panelReTweets.classList.add("d-none");
    }
    if (!e.currentTarget.classList.contains("active")) {
        e.currentTarget.classList.add("active");
        btnTweets.classList.remove("active");
        btnReTweets.classList.remove("active");
    }
});
