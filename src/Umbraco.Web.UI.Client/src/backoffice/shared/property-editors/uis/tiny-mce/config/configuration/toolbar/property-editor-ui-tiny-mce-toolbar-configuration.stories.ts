import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbPropertyEditorUITinyMceToolbarConfigurationElement } from './property-editor-ui-tiny-mce-toolbar-configuration.element';
import './property-editor-ui-tiny-mce-toolbar-configuration.element';

export default {
	title: 'Property Editor UIs/Tiny Mce Toolbar Configuration',
	component: 'umb-property-editor-ui-tiny-mce-toolbar-configuration',
	id: 'umb-property-editor-ui-tiny-mce-toolbar-configuration',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUITinyMceToolbarConfigurationElement> = () =>
	html`<umb-property-editor-ui-tiny-mce-toolbar-configuration></umb-property-editor-ui-tiny-mce-toolbar-configuration>`;
AAAOverview.storyName = 'Overview';
