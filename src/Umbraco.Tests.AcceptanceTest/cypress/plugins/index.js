/// <reference types="cypress" />
// ***********************************************************
// This example plugins/index.js can be used to load plugins
//
// You can change the location of this file or turn off loading
// the plugins file with the 'pluginsFile' configuration option.
//
// You can read more here:
// https://on.cypress.io/plugins-guide
// ***********************************************************

// This function is called when a project is opened or re-opened (e.g. due to
// the project's config changing)
const del = require('del')

/**
 * @type {Cypress.PluginConfig}
 */
module.exports = (on, config) => {
  // `on` is used to hook into various events Cypress emits
  // `config` is the resolved Cypress config
  const baseUrl = config.env.baseUrl || null;

  if (baseUrl) {
    config.baseUrl = baseUrl;
  }

  on('after:spec', (spec, results) => {
    if(results.stats.failures === 0 && results.video) {
      // `del()` returns a promise, so it's important to return it to ensure
      // deleting the video is finished before moving
      return del(results.video)
    }
  })

  return config;
}
