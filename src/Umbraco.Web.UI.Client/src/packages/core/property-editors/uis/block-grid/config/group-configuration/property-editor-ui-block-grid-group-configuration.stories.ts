import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbPropertyEditorUIBlockGridGroupConfigurationElement } from './property-editor-ui-block-grid-group-configuration.element';
import './property-editor-ui-block-grid-group-configuration.element';

export default {
	title: 'Property Editor UIs/Block Grid Group Configuration',
	component: 'umb-property-editor-ui-block-grid-group-configuration',
	id: 'umb-property-editor-ui-block-grid-group-configuration',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUIBlockGridGroupConfigurationElement> = () =>
	html`<umb-property-editor-ui-block-grid-group-configuration></umb-property-editor-ui-block-grid-group-configuration>`;
AAAOverview.storyName = 'Overview';
