import { Meta, Story } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import type { UmbPropertyEditorUIBlockGridBlockConfigurationElement } from './property-editor-ui-block-grid-block-configuration.element.js';
import './property-editor-ui-block-grid-block-configuration.element';

export default {
	title: 'Property Editor UIs/Block Grid Block Configuration',
	component: 'umb-property-editor-ui-block-grid-block-configuration',
	id: 'umb-property-editor-ui-block-grid-block-configuration',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUIBlockGridBlockConfigurationElement> = () =>
	html`<umb-property-editor-ui-block-grid-block-configuration></umb-property-editor-ui-block-grid-block-configuration>`;
AAAOverview.storyName = 'Overview';
