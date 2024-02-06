import type { Meta, StoryObj } from '@storybook/web-components';
import type { UmbPropertyEditorUIBlockGridGroupConfigurationElement } from './property-editor-ui-block-type-group-configuration.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-block-type-group-configuration.element.js';

const meta: Meta<UmbPropertyEditorUIBlockGridGroupConfigurationElement> = {
	title: 'Property Editor UIs/Block Grid Group Configuration',
	component: 'umb-property-editor-ui-block-type-group-configuration',
	id: 'umb-property-editor-ui-block-type-group-configuration',
};

export default meta;
type Story = StoryObj<typeof UmbPropertyEditorUIBlockGridGroupConfigurationElement>;

export const Overview: Story = {
	render: () =>
		html`<umb-property-editor-ui-block-type-group-configuration></umb-property-editor-ui-block-type-group-configuration>`,
};
