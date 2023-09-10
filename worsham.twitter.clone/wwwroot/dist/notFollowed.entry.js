/******/ (() => { // webpackBootstrap
/******/ 	"use strict";
/******/ 	// The require scope
/******/ 	var __webpack_require__ = {};
/******/ 	
/************************************************************************/
/******/ 	/* webpack/runtime/define property getters */
/******/ 	(() => {
/******/ 		// define getter functions for harmony exports
/******/ 		__webpack_require__.d = (exports, definition) => {
/******/ 			for(var key in definition) {
/******/ 				if(__webpack_require__.o(definition, key) && !__webpack_require__.o(exports, key)) {
/******/ 					Object.defineProperty(exports, key, { enumerable: true, get: definition[key] });
/******/ 				}
/******/ 			}
/******/ 		};
/******/ 	})();
/******/ 	
/******/ 	/* webpack/runtime/hasOwnProperty shorthand */
/******/ 	(() => {
/******/ 		__webpack_require__.o = (obj, prop) => (Object.prototype.hasOwnProperty.call(obj, prop))
/******/ 	})();
/******/ 	
/******/ 	/* webpack/runtime/make namespace object */
/******/ 	(() => {
/******/ 		// define __esModule on exports
/******/ 		__webpack_require__.r = (exports) => {
/******/ 			if(typeof Symbol !== 'undefined' && Symbol.toStringTag) {
/******/ 				Object.defineProperty(exports, Symbol.toStringTag, { value: 'Module' });
/******/ 			}
/******/ 			Object.defineProperty(exports, '__esModule', { value: true });
/******/ 		};
/******/ 	})();
/******/ 	
/************************************************************************/
var __webpack_exports__ = {};
/*!*******************************!*\
  !*** ./src/js/notFollowed.js ***!
  \*******************************/
__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   NotFollowed: () => (/* binding */ NotFollowed)
/* harmony export */ });


function _typeof(o) { "@babel/helpers - typeof"; return _typeof = "function" == typeof Symbol && "symbol" == typeof Symbol.iterator ? function (o) { return typeof o; } : function (o) { return o && "function" == typeof Symbol && o.constructor === Symbol && o !== Symbol.prototype ? "symbol" : typeof o; }, _typeof(o); }
function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }
function _defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ("value" in descriptor) descriptor.writable = true; Object.defineProperty(target, _toPropertyKey(descriptor.key), descriptor); } }
function _createClass(Constructor, protoProps, staticProps) { if (protoProps) _defineProperties(Constructor.prototype, protoProps); if (staticProps) _defineProperties(Constructor, staticProps); Object.defineProperty(Constructor, "prototype", { writable: false }); return Constructor; }
function _toPropertyKey(arg) { var key = _toPrimitive(arg, "string"); return _typeof(key) === "symbol" ? key : String(key); }
function _toPrimitive(input, hint) { if (_typeof(input) !== "object" || input === null) return input; var prim = input[Symbol.toPrimitive]; if (prim !== undefined) { var res = prim.call(input, hint || "default"); if (_typeof(res) !== "object") return res; throw new TypeError("@@toPrimitive must return a primitive value."); } return (hint === "string" ? String : Number)(input); }
var NotFollowed = /*#__PURE__*/function () {
  function NotFollowed() {
    _classCallCheck(this, NotFollowed);
    this.setEventListeners();
  }
  _createClass(NotFollowed, [{
    key: "setEventListeners",
    value: function setEventListeners() {
      var _this = this;
      /**
       * Event listener that triggers when the DOM content has been fully loaded.
       * This function fetches a list of not followed users from the API and updates the UI with suggested users.
       */
      document.addEventListener("DOMContentLoaded", function () {
        // Select the input element for the anti-forgery token
        var tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');

        // Proceed if the anti-forgery token input is found
        if (tokenInput) {
          var token = tokenInput.value;

          // Fetch the list of not followed users from the endpoint
          fetch("/api/follows/notfollowed").then(function (response) {
            return response.json();
          }) // Parse the response as JSON
          .then(function (data) {
            // Update the UI with suggested users using the fetched data and token
            _this.updateUiWithListOfSuggestedUsers(data, token);
          })["catch"](function (error) {
            // Log and handle the error if fetching data fails
            console.error("Error:", error);
          });
        }
      });
    }

    /**
     * Creates a follow form for a user with the provided information.
     * @param {Object} user - The user object containing user information.
     * @param {string} token - The anti-forgery token used for form submissions.
     * @returns {HTMLElement} The created form element for following the user.
     */
  }, {
    key: "createFollowForm",
    value: function createFollowForm(user, token) {
      var form = document.createElement("form");
      form.classList.add("card-body");
      form.setAttribute("action", "/Follows/Create");
      form.setAttribute("method", "post");

      // Add the anti-forgery token as a hidden input
      var tokenInput = document.createElement("input");
      tokenInput.setAttribute("type", "hidden");
      tokenInput.setAttribute("name", "__RequestVerificationToken");
      tokenInput.setAttribute("value", token);
      var h5 = document.createElement("h5");
      h5.classList.add("card-title", "d-inline");
      h5.innerText = user.userName;
      var button = document.createElement("button");
      button.classList.add("btn", "btn-primary--dark", "d-inline", "rounded-pill", "float-end");
      button.innerText = "Follow";
      button.setAttribute("type", "submit");
      var userIdInput = document.createElement("input");
      userIdInput.setAttribute("type", "hidden");
      userIdInput.setAttribute("name", "userId");
      userIdInput.setAttribute("value", user.id);
      form.appendChild(tokenInput);
      form.appendChild(h5);
      form.appendChild(userIdInput);
      form.appendChild(button);
      return form;
    }

    /**
     * Updates the user interface by displaying a list of suggested users and creating follow forms.
     * @param {Array} users - An array of user objects representing suggested users.
     * @param {string} token - The anti-forgery token used for form submissions.
     */
  }, {
    key: "updateUiWithListOfSuggestedUsers",
    value: function updateUiWithListOfSuggestedUsers(users, token) {
      var _this2 = this;
      var suggestedUsersCard = document.getElementById("suggestedUsersCard");
      var suggestedUsers = document.getElementById("suggestedUsers");
      // Clear existing content in the suggested users card
      if (suggestedUsers && suggestedUsersCard) {
        suggestedUsers.innerHTML = "";
        // Display suggested users and create follow forms if there are users
        if (users.length > 0) {
          // Remove the "d-none" class to make the suggested users card visible
          suggestedUsersCard.classList.remove("d-none");

          // Create follow forms for each suggested user and append them to the card
          users.forEach(function (user) {
            var form = _this2.createFollowForm(user, token);
            suggestedUsersCard.appendChild(form);
          });
        }
      }
    }
  }]);
  return NotFollowed;
}();
var notFollowed = new NotFollowed();
/******/ })()
;
//# sourceMappingURL=notFollowed.entry.js.map