export class LandingPage {
	constructor() {
		// Initialize any necessary variables or properties here
	}

	initSignIn() {
		if (didUserRegister) {
			setTimeout(() => {
				document.getElementById("btnSignIn").click();
			}, 1000);
		}
	}
}

const landingPage = new LandingPage();
landingPage.initSignIn();