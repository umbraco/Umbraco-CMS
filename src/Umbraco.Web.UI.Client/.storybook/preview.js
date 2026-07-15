import lightCss from '@umbraco-ui/uui/themes/light.css?inline';
import darkCss from '@umbraco-ui/uui/themes/dark.css?inline';
import highContrastCss from '@umbraco-ui/uui/themes/high-contrast.css?inline';
import '../src/css/umb-css.css';

import 'element-internals-polyfill';
import '@umbraco-ui/uui';

import { html } from 'lit';
import { setCustomElements } from '@storybook/web-components-vite';

import { startMockServiceWorker } from '../mocks';

import '../src/libs/controller-api/controller-host-provider.element';
import { UmbExtensionsApiInitializer } from '../src/libs/extension-api/index';
import { UmbModalManagerContext } from '../src/packages/core/modal';
import { umbExtensionsRegistry } from '../src/packages/core/extension-registry';
import { UmbIconRegistry } from '../src/packages/core/icon-registry/icon.registry';
import { UmbLitElement } from '../src/packages/core/lit-element';
import { umbLocalizationRegistry } from '../src/packages/core/localization';
import customElementManifests from '../custom-elements.json';
import icons from '../src/packages/core/icon-registry/icons';

import '../src/libs/context-api/provide/context-provider.element';
import '../src/packages/core/components';

import { manifests as blockManifests } from '../src/packages/block/umbraco-package';
import { manifests as clipboardManifests } from '../src/packages/clipboard/umbraco-package';
import { manifests as codeEditorManifests } from '../src/packages/code-editor/umbraco-package';
import { manifests as contentManifests } from '../src/packages/content/umbraco-package';
import { manifests as coreManifests } from '../src/packages/core/manifests';
import { manifests as dataTypeManifests } from '../src/packages/data-type/umbraco-package';
import { manifests as dictionaryManifests } from '../src/packages/dictionary/umbraco-package';
import { manifests as documentManifests } from '../src/packages/documents/umbraco-package';
import { manifests as embeddedMediaManifests } from '../src/packages/embedded-media/umbraco-package';
import { manifests as extensionInsightsManifests } from '../src/packages/extension-insights/umbraco-package';
import { manifests as healthCheckManifests } from '../src/packages/health-check/umbraco-package';
import { manifests as helpManifests } from '../src/packages/help/umbraco-package';
import { manifests as languageManifests } from '../src/packages/language/umbraco-package';
import { manifests as logViewerManifests } from '../src/packages/log-viewer/umbraco-package';
import { manifests as markdownEditorManifests } from '../src/packages/markdown-editor/umbraco-package';
import { manifests as mediaManifests } from '../src/packages/media/umbraco-package';
import { manifests as memberManifests } from '../src/packages/members/umbraco-package';
import { manifests as modelsBuilderManifests } from '../src/packages/models-builder/umbraco-package';
import { manifests as multiUrlPickerManifests } from '../src/packages/multi-url-picker/umbraco-package';
import { manifests as packageManifests } from '../src/packages/packages/umbraco-package';
import { manifests as performanceProfilingManifests } from '../src/packages/performance-profiling/umbraco-package';
import { manifests as propertyEditorManifests } from '../src/packages/property-editors/umbraco-package';
import { manifests as publishCacheManifests } from '../src/packages/publish-cache/umbraco-package';
import { manifests as relationsManifests } from '../src/packages/relations/umbraco-package';
import { manifests as rteManifests } from '../src/packages/rte/umbraco-package';
import { manifests as segmentManifests } from '../src/packages/segment/umbraco-package';
import { manifests as settingsManifests } from '../src/packages/settings/umbraco-package';
import { manifests as staticFileManifests } from '../src/packages/static-file/umbraco-package';
import { manifests as sysInfoManifests } from '../src/packages/sysinfo/umbraco-package';
import { manifests as tagManifests } from '../src/packages/tags/umbraco-package';
import { manifests as telemetryManifests } from '../src/packages/telemetry/umbraco-package';
import { manifests as templatingManifests } from '../src/packages/templating/umbraco-package';
import { manifests as tipTapManifests } from '../src/packages/tiptap/umbraco-package';
import { manifests as translationManifests } from '../src/packages/translation/umbraco-package';
import { manifests as ufmManifests } from '../src/packages/ufm/umbraco-package';
//import { manifests as umbracoNewsManifests } from '../src/packages/umbraco-news/umbraco-package';
import { manifests as userManifests } from '../src/packages/user/umbraco-package';
import { manifests as webhookManifests } from '../src/packages/webhook/umbraco-package';

import { UmbNotificationContext } from '../src/packages/core/notification';
import { UmbContextBase } from '../src/libs/class-api/index';
import { UmbBooleanState } from '../src/libs/observable-api/index';
import { UMB_APP_LANGUAGE_CONTEXT } from '../src/packages/language/constants';

// MSW
startMockServiceWorker({ serviceWorker: { url: (import.meta.env.VITE_BASE_PATH ?? '/') + 'mockServiceWorker.js' } });

const themes = {
	light: lightCss,
	dark: darkCss,
	'high-contrast': highContrastCss,
};

let themeStyleEl = null;

export const globalTypes = {
	theme: {
		name: 'Theme',
		defaultValue: 'light',
		toolbar: {
			icon: 'paintbrush',
			items: [
				{ value: 'light', title: 'Light' },
				{ value: 'dark', title: 'Dark' },
				{ value: 'high-contrast', title: 'High Contrast' },
			],
			dynamicTitle: true,
		},
	},
};

function applyTheme(theme) {
	if (!themeStyleEl) {
		themeStyleEl = document.createElement('style');
		themeStyleEl.id = 'uui-theme';
		document.head.appendChild(themeStyleEl);
	}
	themeStyleEl.textContent = themes[theme] ?? lightCss;
}

class UmbStoryBookAuthContext extends UmbContextBase {
	#isAuthorized = new UmbBooleanState(true);
	isAuthorized = this.#isAuthorized.asObservable();

	constructor(host) {
		super(host, 'UmbAuthContext');
	}
}

class UmbStoryBookElement extends UmbLitElement {
	_umbIconRegistry = new UmbIconRegistry();

	#manifests = [
		...blockManifests,
		...clipboardManifests,
		...codeEditorManifests,
		...contentManifests,
		...coreManifests,
		...dataTypeManifests,
		...dictionaryManifests,
		...documentManifests,
		...embeddedMediaManifests,
		...extensionInsightsManifests,
		...healthCheckManifests,
		...helpManifests,
		...languageManifests,
		...logViewerManifests,
		...markdownEditorManifests,
		...mediaManifests,
		...memberManifests,
		...modelsBuilderManifests,
		...multiUrlPickerManifests,
		...packageManifests,
		...performanceProfilingManifests,
		...propertyEditorManifests,
		...publishCacheManifests,
		...relationsManifests,
		...rteManifests,
		...segmentManifests,
		...settingsManifests,
		...staticFileManifests,
		...sysInfoManifests,
		...tagManifests,
		...telemetryManifests,
		...templatingManifests,
		...tipTapManifests,
		...translationManifests,
		...ufmManifests,
		//...umbracoNewsManifests,
		...userManifests,
		...webhookManifests,
	];

	constructor() {
		super();
		new UmbExtensionsApiInitializer(this, umbExtensionsRegistry, 'globalContext', [this]);
		new UmbExtensionsApiInitializer(this, umbExtensionsRegistry, 'store', [this]);
		new UmbExtensionsApiInitializer(this, umbExtensionsRegistry, 'itemStore', [this]);

		this._umbIconRegistry.setIcons(icons);
		this._umbIconRegistry.attach(this);
		this._registerExtensions(this.#manifests);

		new UmbStoryBookAuthContext(this);
		new UmbModalManagerContext(this);
		new UmbNotificationContext(this);

		umbLocalizationRegistry.loadLanguage('en'); // register default language

		this.consumeContext(UMB_APP_LANGUAGE_CONTEXT, (appLanguageContext) => {
			appLanguageContext?.setLanguage('en'); // set default language
		});
	}

	_registerExtensions(manifests) {
		manifests.forEach((manifest) => {
			if (umbExtensionsRegistry.isRegistered(manifest.alias)) return;
			umbExtensionsRegistry.register(manifest);
		});
	}

	render() {
		return html`<slot></slot>
			<umb-backoffice-modal-container></umb-backoffice-modal-container>
			<umb-backoffice-notification-container></umb-backoffice-notification-container>`;
	}
}

customElements.define('umb-storybook', UmbStoryBookElement);

const themeProvider = (story, context) => {
	applyTheme(context.globals['theme'] ?? 'light');
	return story();
};
const storybookProvider = (story) => html` <umb-storybook>${story()}</umb-storybook> `;

// Provide the MSW addon decorator globally
export const decorators = [themeProvider, storybookProvider];

export const parameters = {
	docs: {
		source: {
			excludeDecorators: true,
			format: 'html', // see storybook docs for more info on this format https://storybook.js.org/docs/api/doc-blocks/doc-block-source#format
		},
	},
	options: {
		storySort: {
			method: 'alphabetical',
			includeNames: true,
			order: ['Generic Components', 'Extension Type', 'Entity', 'Guides'],
		},
	},
	controls: {
		expanded: true,
		matchers: {
			color: /(background|color)$/i,
			date: /Date$/,
		},
	},
	backgrounds: {
		options: {
			greyish: {
				name: 'Greyish',
				value: '#F3F3F5',
			},

			white: {
				name: 'White',
				value: '#ffffff',
			}
		}
	},
};

setCustomElements(customElementManifests);
export const tags = ['autodocs'];

export const initialGlobals = {
	backgrounds: {
		value: 'greyish'
	}
};
