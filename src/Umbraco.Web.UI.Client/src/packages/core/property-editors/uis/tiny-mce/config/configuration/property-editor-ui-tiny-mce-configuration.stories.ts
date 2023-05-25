import { Meta, Story } from '@storybook/web-components';
import type { UmbPropertyEditorUITinyMceConfigurationElement } from './property-editor-ui-tiny-mce-configuration.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-tiny-mce-configuration.element.js';

export default {
	title: 'Property Editor UIs/Tiny Mce Configuration',
	component: 'umb-property-editor-ui-tiny-mce-configuration',
	id: 'umb-property-editor-ui-tiny-mce-configuration',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUITinyMceConfigurationElement> = () =>
	html`<umb-property-editor-ui-tiny-mce-configuration></umb-property-editor-ui-tiny-mce-configuration>`;
AAAOverview.storyName = 'Overview';
