import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbPropertyEditorUITinyMceConfigurationElement } from './property-editor-ui-tiny-mce-configuration.element';
import './property-editor-ui-tiny-mce-configuration.element';

export default {
	title: 'Property Editor UIs/Tiny Mce Configuration',
	component: 'umb-property-editor-ui-tiny-mce-configuration',
	id: 'umb-property-editor-ui-tiny-mce-configuration',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUITinyMceConfigurationElement> = () =>
	html`<umb-property-editor-ui-tiny-mce-configuration></umb-property-editor-ui-tiny-mce-configuration>`;
AAAOverview.storyName = 'Overview';
