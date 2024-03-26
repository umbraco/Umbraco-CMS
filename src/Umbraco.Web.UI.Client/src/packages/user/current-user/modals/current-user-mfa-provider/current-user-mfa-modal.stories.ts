import type { Meta, StoryObj } from '@storybook/web-components';
import type { UmbCurrentUserMfaProviderModalElement } from './current-user-mfa-provider-modal.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';
import { UmbServerExtensionRegistrator } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

import './current-user-mfa-provider-modal.element.js';

class UmbServerExtensionsHostElement extends UmbLitElement {
	constructor() {
		super();
		new UmbServerExtensionRegistrator(this, umbExtensionsRegistry).registerAllExtensions();
	}

	render() {
		return html`<slot></slot>`;
	}
}

if (window.customElements.get('umb-server-extensions-host') === undefined) {
	customElements.define('umb-server-extensions-host', UmbServerExtensionsHostElement);
}

const meta: Meta<UmbCurrentUserMfaProviderModalElement> = {
	id: 'umb-current-user-mfa-provider-modal',
	title: 'Current User/Modals/MFA Provider Modal',
	component: 'umb-current-user-mfa-provider-modal',
	decorators: [
		(Story) =>
			html`<umb-server-extensions-host style="width: 500px; height: 500px;">${Story()}</umb-server-extensions-host>`,
	],
	args: {
		data: {
			providerName: 'SMS',
		},
	},
	parameters: {
		layout: 'centered',
		actions: {
			disabled: true,
		},
	},
};

export default meta;

type Story = StoryObj<UmbCurrentUserMfaProviderModalElement>;

export const Overview: Story = {};
