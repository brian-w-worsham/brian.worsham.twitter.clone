export class ProfilePage {
	constructor() {
		this.btnTweets = document.getElementById("btnTweets");
		this.btnReTweets = document.getElementById("btnReTweets");
		this.btnLikes = document.getElementById("btnLikes");
		this.panelTweets = document.getElementById("panelTweets");
		this.panelReTweets = document.getElementById("panelReTweets");
		this.panelLikes = document.getElementById("panelLikes");

		if (this.btnTweets && this.btnReTweets && this.btnLikes && this.panelTweets && this.panelReTweets && this.panelLikes) {
			this.addEventListeners();
		}
	}

	addEventListeners() {
		this.btnTweets.addEventListener("click", (e) => {
			this.showPanel(this.panelTweets);
			this.toggleButtonActive(this.btnTweets);
		});

		this.btnReTweets.addEventListener("click", (e) => {
			this.showPanel(this.panelReTweets);
			this.toggleButtonActive(this.btnReTweets);
		});

		this.btnLikes.addEventListener("click", (e) => {
			this.showPanel(this.panelLikes);
			this.toggleButtonActive(this.btnLikes);
		});
	}

	showPanel(panel) {
		const panels = [this.panelTweets, this.panelReTweets, this.panelLikes];
		panels.forEach((p) => {
			if (p === panel) {
				p.classList.remove("d-none");
			} else {
				p.classList.add("d-none");
			}
		});
	}

	toggleButtonActive(button) {
		const buttons = [this.btnTweets, this.btnReTweets, this.btnLikes];
		buttons.forEach((btn) => {
			if (btn === button) {
				btn.classList.add("active");
			} else {
				btn.classList.remove("active");
			}
		});
	}
}

const profilePage = new ProfilePage();