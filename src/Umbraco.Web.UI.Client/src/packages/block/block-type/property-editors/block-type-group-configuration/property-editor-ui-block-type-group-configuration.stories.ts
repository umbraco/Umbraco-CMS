import { Meta, Story } from '@storybook/web-components';
import type { UmbPropertyEditorUIBlockGridGroupConfigurationElement } from './property-editor-ui-block-type-group-configuration.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-block-grid-group-configuration.element.js';

export default {
	title: 'Property Editor UIs/Block Grid Group Configuration',
	component: 'umb-property-editor-ui-block-type-group-configuration',
	id: 'umb-property-editor-ui-block-type-group-configuration',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUIBlockGridGroupConfigurationElement> = () =>
	html`<umb-property-editor-ui-block-type-group-configuration></umb-property-editor-ui-block-type-group-configuration>`;
AAAOverview.storyName = 'Overview';
