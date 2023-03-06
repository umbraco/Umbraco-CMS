import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbPropertyEditorUITinyMceMaxImageSizeConfigurationElement } from './property-editor-ui-tiny-mce-maximagesize-configuration.element';
import './property-editor-ui-tiny-mce-maximagesize-configuration.element';

export default {
	title: 'Property Editor UIs/Tiny Mce Max Image Size Configuration',
	component: 'umb-property-eDitor-ui-tiny-mce-maximagesize-configuration',
	id: 'umb-property-editor-ui-tiny-mce-maximagesize-configuration',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUITinyMceMaxImageSizeConfigurationElement> = () =>
	html`<umb-property-editor-ui-tiny-mce-maximagesize-configuration></umb-property-editor-ui-tiny-mce-maximagesize-configuration>`;
AAAOverview.storyName = 'Overview';
