import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbPropertyEditorUITinyMceDimensionsConfigurationElement } from './property-editor-ui-tiny-mce-dimensions-configuration.element';
import './property-editor-ui-tiny-mce-dimensions-configuration.element';

export default {
	title: 'Property Editor UIs/Tiny Mce Dimensions Configuration',
	component: 'umb-property-eDitor-ui-tiny-mce-dimensions-configuration',
	id: 'umb-property-editor-ui-tiny-mce-dimensions-configuration',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUITinyMceDimensionsConfigurationElement> = () =>
	html`<umb-property-editor-ui-tiny-mce-dimensions-configuration></umb-property-editor-ui-tiny-mce-dimensions-configuration>`;
AAAOverview.storyName = 'Overview';
