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
  !*** ./src/js/profilePage.js ***!
  \*******************************/
__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   ProfilePage: () => (/* binding */ ProfilePage)
/* harmony export */ });
function _typeof(o) { "@babel/helpers - typeof"; return _typeof = "function" == typeof Symbol && "symbol" == typeof Symbol.iterator ? function (o) { return typeof o; } : function (o) { return o && "function" == typeof Symbol && o.constructor === Symbol && o !== Symbol.prototype ? "symbol" : typeof o; }, _typeof(o); }
function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }
function _defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ("value" in descriptor) descriptor.writable = true; Object.defineProperty(target, _toPropertyKey(descriptor.key), descriptor); } }
function _createClass(Constructor, protoProps, staticProps) { if (protoProps) _defineProperties(Constructor.prototype, protoProps); if (staticProps) _defineProperties(Constructor, staticProps); Object.defineProperty(Constructor, "prototype", { writable: false }); return Constructor; }
function _toPropertyKey(arg) { var key = _toPrimitive(arg, "string"); return _typeof(key) === "symbol" ? key : String(key); }
function _toPrimitive(input, hint) { if (_typeof(input) !== "object" || input === null) return input; var prim = input[Symbol.toPrimitive]; if (prim !== undefined) { var res = prim.call(input, hint || "default"); if (_typeof(res) !== "object") return res; throw new TypeError("@@toPrimitive must return a primitive value."); } return (hint === "string" ? String : Number)(input); }
var ProfilePage = /*#__PURE__*/function () {
  function ProfilePage() {
    _classCallCheck(this, ProfilePage);
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
  _createClass(ProfilePage, [{
    key: "addEventListeners",
    value: function addEventListeners() {
      var _this = this;
      this.btnTweets.addEventListener("click", function (e) {
        _this.showPanel(_this.panelTweets);
        _this.toggleButtonActive(_this.btnTweets);
      });
      this.btnReTweets.addEventListener("click", function (e) {
        _this.showPanel(_this.panelReTweets);
        _this.toggleButtonActive(_this.btnReTweets);
      });
      this.btnLikes.addEventListener("click", function (e) {
        _this.showPanel(_this.panelLikes);
        _this.toggleButtonActive(_this.btnLikes);
      });
    }
  }, {
    key: "showPanel",
    value: function showPanel(panel) {
      var panels = [this.panelTweets, this.panelReTweets, this.panelLikes];
      panels.forEach(function (p) {
        if (p === panel) {
          p.classList.remove("d-none");
        } else {
          p.classList.add("d-none");
        }
      });
    }
  }, {
    key: "toggleButtonActive",
    value: function toggleButtonActive(button) {
      var buttons = [this.btnTweets, this.btnReTweets, this.btnLikes];
      buttons.forEach(function (btn) {
        if (btn === button) {
          btn.classList.add("active");
        } else {
          btn.classList.remove("active");
        }
      });
    }
  }]);
  return ProfilePage;
}();
var profilePage = new ProfilePage();
/******/ })()
;
//# sourceMappingURL=profilePage.entry.js.map