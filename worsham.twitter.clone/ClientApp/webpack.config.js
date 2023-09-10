const webpack = require("webpack");
const path = require("path");
MiniCssExtractPlugin = require("mini-css-extract-plugin");

module.exports = {
	entry: {
		bootstrap_js: "./src/js/bootstrap_js.js",
		landingPage: "./src/js/landingPage.js",
		login: "./src/js/login.js",
		notFollowed: "./src/js/notFollowed.js",
		profilePage: "./src/js/profilePage.js",
		registrationForm: "./src/js/registrationForm.js",
		site: "./src/js/site.js",
		validation: "./src/js/validation.js",
	},
	output: {
		filename: "[name].entry.js",
		path: path.resolve(__dirname, "..", "wwwroot", "dist"),
	},
	devtool: "source-map",
	mode: "development",
	module: {
		rules: [
			{
				test: /\.m?js$/,
				exclude: /node_modules/,
				use: {
					loader: "babel-loader",
					options: {
						presets: ["@babel/preset-env"], // babel allows us to write our JS with modern style and babel will convert it to ECMA5 for older browser support
						plugins: ["@babel/plugin-transform-class-properties"], // This plugin allows you to use class properties.
					},
				},
			},
			{
				test: /\.css$/,
				use: [{ loader: MiniCssExtractPlugin.loader }, "css-loader"],
			},
			{
				test: /\.(eot|woff(2)?|ttf|otf|svg)$/i,
				type: "asset",
			},
		],
	},
	plugins: [
		// Provide jQuery to all modules that require it
		new webpack.ProvidePlugin({
			$: "jquery",
			jQuery: "jquery",
		}),
		new MiniCssExtractPlugin({
			filename: "[name].css",
		}),
	],
};
