import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbPropertyEditorUITinyMceModeConfigurationElement } from './property-editor-ui-tiny-mce-mode-configuration.element';
import './property-editor-ui-tiny-mce-mode-configuration.element';

export default {
	title: 'Property Editor UIs/Tiny Mce Mode Configuration',
	component: 'umb-property-editor-ui-tiny-mce-mode-configuration',
	id: 'umb-property-editor-ui-tiny-mce-mode-configuration',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUITinyMceModeConfigurationElement> = () =>
	html`<umb-property-editor-ui-tiny-mce-mode-configuration></umb-property-editor-ui-tiny-mce-mode-configuration>`;
AAAOverview.storyName = 'Overview';
