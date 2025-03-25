// timesnapper-supabase-plugin.js
var { createClient } = require("@supabase/supabase-js");
var fs = require("fs");
var path = require("path");
var readline = require("readline");
var ENV_PATH = path.join(__dirname, ".env");
if (fs.existsSync(ENV_PATH)) {
  try {
    require("dotenv").config({ path: ENV_PATH });
  } catch (err) {
    console.error("Failed to load .env file:", err);
  }
}
var SUPABASE_URL = process.env.SUPABASE_URL || "YOUR_SUPABASE_URL";
var SUPABASE_KEY = process.env.SUPABASE_KEY || "YOUR_SUPABASE_KEY";
var supabase = createClient(SUPABASE_URL, SUPABASE_KEY);
function onMetadataCaptured(metadata) {
  const windowTitle = metadata.window_title || "";
  const application = metadata.application || "";
  supabase.from("timesnapper_metadata").insert([{ window_title: windowTitle, application, metadata }]).then((response) => {
    console.log("Supabase insert response:", response);
  }).catch((error) => {
    console.error("Error inserting to Supabase:", error);
  });
}
function init() {
  console.log("Initializing Supabase Metadata Plugin");
  if (typeof Timesnapper !== "undefined" && Timesnapper.on) {
    Timesnapper.on("metadataCaptured", onMetadataCaptured);
  } else {
    console.warn("Timesnapper API not available. Ensure TimeSnapper is running and supports plugins.");
  }
}
function shutdown() {
  console.log("Shutting down Supabase Metadata Plugin");
}
function getName() {
  return "Supabase Metadata Plugin";
}
function getVersion() {
  return "1.0.0";
}
function showConfigurationMenu() {
  const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout,
    terminal: true
  });
  rl.question("Enter Supabase URL: ", (supabaseUrl) => {
    rl.question("Enter Supabase Key: ", (supabaseKey) => {
      const envContent = `SUPABASE_URL=${supabaseUrl.trim()}
SUPABASE_KEY=${supabaseKey.trim()}
`;
      fs.writeFile(ENV_PATH, envContent, "utf8", (err) => {
        if (err) {
          console.error("Error writing .env file:", err);
        } else {
          console.log(".env file created/updated successfully with the provided credentials.");
        }
        rl.close();
        process.exit(0);
      });
    });
  });
}
function showMenu() {
  console.log("===== Supabase Metadata Plugin Configuration Menu =====");
  console.log("1. Configure Supabase Credentials (creates/updates .env file)");
  console.log("2. Exit");
  const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout,
    terminal: true
  });
  rl.question("Select an option: ", (answer) => {
    switch (answer.trim()) {
      case "1":
        showConfigurationMenu();
        break;
      default:
        console.log("Exiting configuration menu.");
        rl.close();
        process.exit(0);
    }
  });
}
if (require.main === module && process.argv[2] === "configure") {
  showMenu();
}
module.exports = {
  init,
  shutdown,
  getName,
  getVersion,
  showConfigurationMenu,
  showMenu
};
