import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbPropertyEditorUITinyMceStylesheetsConfigurationElement } from './property-editor-ui-tiny-mce-stylesheets-configuration.element';
import './property-editor-ui-tiny-mce-stylesheets-configuration.element';

export default {
	title: 'Property Editor UIs/Tiny Mce Stylesheets Configuration',
	component: 'umb-property-editor-ui-tiny-mce-stylesheets-configuration',
	id: 'umb-property-editor-ui-tiny-mce-stylesheets-configuration',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUITinyMceStylesheetsConfigurationElement> = () =>
	html`<umb-property-editor-ui-tiny-mce-stylesheets-configuration></umb-property-editor-ui-tiny-mce-stylesheets-configuration>`;
AAAOverview.storyName = 'Overview';
