const tsconfigPaths = require('vite-tsconfig-paths').default;
const {
  mergeConfig
} = require('vite');
module.exports = {
  stories: ['../@(src|libs|apps)/**/*.@(mdx|stories.@(js|jsx|ts|tsx))'],
  addons: ['@storybook/addon-links', '@storybook/addon-essentials', '@storybook/addon-a11y'],
  framework: {
    name: '@storybook/web-components-vite',
    options: {}
  },
  features: {
    previewMdx2: true,
    storyStoreV7: true
  },
  staticDirs: ['../public-assets'],
  async viteFinal(config, {
    configType
  }) {
    return mergeConfig(config, {
      // customize the Vite config here
      plugins: [tsconfigPaths()]
    });
  },
  docs: {
    autodocs: true
  }
};