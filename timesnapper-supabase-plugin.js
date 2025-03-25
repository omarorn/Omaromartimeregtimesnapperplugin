/*
 * Supabase Metadata Plugin for TimeSnapper
 * This plugin listens for metadata events and sends them to Supabase.
 */

const { createClient } = require('@supabase/supabase-js');
const fs = require('fs');
const path = require('path');
const readline = require('readline');

// Path for .env file in the plugin folder
const ENV_PATH = path.join(__dirname, '.env');

// Load environment variables from .env if it exists
if (fs.existsSync(ENV_PATH)) {
  try {
    require('dotenv').config({ path: ENV_PATH });
  } catch (err) {
    console.error('Failed to load .env file:', err);
  }
}

const SUPABASE_URL = process.env.SUPABASE_URL;
const SUPABASE_KEY = process.env.SUPABASE_KEY;
let supabase;

if (SUPABASE_URL && SUPABASE_URL.startsWith('http')) {
  supabase = createClient(SUPABASE_URL, SUPABASE_KEY);
} else {
  console.warn('Supabase not configured properly. Please run the configuration menu to set a valid URL and key.');
  supabase = null;
}

/**
 * Handles metadata captured events by sending data to Supabase.
 * @param {Object} metadata - Metadata captured by TimeSnapper.
 */
function onMetadataCaptured(metadata) {
  if (!supabase) {
    console.error('Supabase client is not configured. Aborting data send.');
    return;
  }
  const windowTitle = metadata.window_title || '';
  const application = metadata.application || '';
  supabase
    .from('timesnapper_metadata')
    .insert([{ window_title: windowTitle, application: application, metadata }])
    .then(response => {
      console.log('Supabase insert response:', response);
    })
    .catch(error => {
      console.error('Error inserting to Supabase:', error);
    });
}

/**
 * Initializes the plugin.
 * This function is called by TimeSnapper when loading plugins.
 */
function init() {
  console.log('Initializing Supabase Metadata Plugin');
  if (typeof Timesnapper !== 'undefined' && Timesnapper.on) {
    Timesnapper.on('metadataCaptured', onMetadataCaptured);
  } else {
    console.warn('Timesnapper API not available. Ensure TimeSnapper is running and supports plugins.');
  }
}

/**
 * Shuts down the plugin.
 * This function is called by TimeSnapper when unloading plugins.
 */
function shutdown() {
  console.log('Shutting down Supabase Metadata Plugin');
  // Unregister events if necessary.
}

/**
 * Returns the plugin name.
 */
function getName() {
  return 'Supabase Metadata Plugin';
}

/**
 * Returns the plugin version.
 */
function getVersion() {
  return '1.0.0';
}

/**
 * Shows an interactive configuration menu to set Supabase credentials.
 * It creates or updates the .env file with the provided values.
 */
function showConfigurationMenu() {
  const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout,
    terminal: true
  });

  rl.question('Enter Supabase URL (must start with http:// or https://): ', supabaseUrl => {
    rl.question('Enter Supabase Key: ', supabaseKey => {
      const envContent = `SUPABASE_URL=${supabaseUrl.trim()}\nSUPABASE_KEY=${supabaseKey.trim()}\n`;
      fs.writeFile(ENV_PATH, envContent, 'utf8', err => {
        if (err) {
          console.error('Error writing .env file:', err);
        } else {
          console.log('.env file created/updated successfully with the provided credentials.');
        }
        rl.close();
        process.exit(0); // Exit after configuration is complete
      });
    });
  });
}

/**
 * Displays a simple configuration menu.
 */
function showMenu() {
  console.log('===== Supabase Metadata Plugin Configuration Menu =====');
  console.log('1. Configure Supabase Credentials (creates/updates .env file)');
  console.log('2. Exit');
  const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout,
    terminal: true
  });
  rl.question('Select an option: ', answer => {
    switch (answer.trim()) {
      case '1':
        showConfigurationMenu();
        break;
      default:
        console.log('Exiting configuration menu.');
        rl.close();
        process.exit(0);
    }
  });
}

// If run directly with "configure" argument, show the configuration menu.
if (require.main === module && process.argv[2] === 'configure') {
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