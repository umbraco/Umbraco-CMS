import '@umbraco-ui/uui-css/dist/uui-css.css';
import '../src/css/umb-css.css';

import 'element-internals-polyfill';
import '@umbraco-ui/uui';

import { html } from 'lit';
import { setCustomElements } from '@storybook/web-components';

import { startMockServiceWorker } from '../src/mocks';

import '../src/libs/controller-api/controller-host-provider.element';
import { UmbModalManagerContext } from '../src/packages/core/modal';
import { UmbDataTypeTreeStore } from '../src/packages/data-type/tree/data-type-tree.store';
import { UmbDocumentDetailStore } from '../src/packages/documents/documents/repository/detail/document-detail.store';
import { UmbDocumentTreeStore } from '../src/packages/documents/documents/tree/document-tree.store';
import { UmbCurrentUserStore } from '../src/packages/user/current-user/repository/current-user.store';
import { umbExtensionsRegistry } from '../src/packages/core/extension-registry';
import { UmbIconRegistry } from '../src/packages/core/icon-registry/icon.registry';
import { UmbLitElement } from '../src/packages/core/lit-element';
import { umbLocalizationRegistry } from '../src/packages/core/localization';
import customElementManifests from '../dist-cms/custom-elements.json';
import icons from '../src/packages/core/icon-registry/icons';

import '../src/libs/context-api/provide/context-provider.element';
import '../src/packages/core/components';

import { manifests as documentManifests } from '../src/packages/documents/manifests';
import { manifests as localizationManifests } from '../src/packages/core/localization/manifests';
import { UmbNotificationContext } from '../src/packages/core/notification';

// MSW
startMockServiceWorker({ serviceWorker: { url: (import.meta.env.VITE_BASE_PATH ?? '/') + 'mockServiceWorker.js' } });

class UmbStoryBookElement extends UmbLitElement {
	_umbIconRegistry = new UmbIconRegistry();

	constructor() {
		super();
		this._umbIconRegistry.setIcons(icons);
		this._umbIconRegistry.attach(this);
		this._registerExtensions(documentManifests);
		new UmbModalManagerContext(this);
		new UmbCurrentUserStore(this);
		new UmbNotificationContext(this);

		this._registerExtensions(localizationManifests);
		umbLocalizationRegistry.loadLanguage('en-us'); // register default language
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

const storybookProvider = (story) => html` <umb-storybook>${story()}</umb-storybook> `;

const dataTypeStoreProvider = (story) => html`
	<umb-controller-host-provider .create=${(host) => new UmbDataTypeTreeStore(host)}
		>${story()}</umb-controller-host-provider
	>
`;

const documentStoreProvider = (story) => html`
	<umb-controller-host-provider .create=${(host) => new UmbDocumentDetailStore(host)}
		>${story()}</umb-controller-host-provider
	>
`;

const documentTreeStoreProvider = (story) => html`
	<umb-controller-host-provider .create=${(host) => new UmbDocumentTreeStore(host)}
		>${story()}</umb-controller-host-provider
	>
`;

// Provide the MSW addon decorator globally
export const decorators = [documentStoreProvider, documentTreeStoreProvider, dataTypeStoreProvider, storybookProvider];

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
			order: [
				'Guides',
				[
					'Getting started',
					'Extending the Backoffice',
					[
						'Intro',
						'Registration',
						'Header Apps',
						'Sections',
						['Intro', 'Sidebar', 'Views', '*'],
						'Entity Actions',
						'Workspaces',
						['Intro', 'Views', 'Actions', '*'],
						'Property Editors',
						'Repositories',
						'*',
					],
					'*',
				],
				'*',
			],
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
		default: 'Greyish',
		values: [
			{
				name: 'Greyish',
				value: '#F3F3F5',
			},
			{
				name: 'White',
				value: '#ffffff',
			},
		],
	},
};

setCustomElements(customElementManifests);
export const tags = ['autodocs'];
