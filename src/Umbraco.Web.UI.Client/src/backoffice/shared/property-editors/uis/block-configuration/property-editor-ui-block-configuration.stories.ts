import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbPropertyEditorUIBlockConfigurationElement } from './property-editor-ui-block-configuration.element';
import './property-editor-ui-block-configuration.element';

export default {
	title: 'Property Editor UIs/Block Configuration',
	component: 'umb-property-editor-ui-block-configuration',
	id: 'umb-property-editor-ui-block-configuration',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUIBlockConfigurationElement> = () =>
	html`<umb-property-editor-ui-block-configuration></umb-property-editor-ui-block-configuration>`;
AAAOverview.storyName = 'Overview';
