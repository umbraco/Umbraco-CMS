import { Meta, Story } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import type { UmbPropertyEditorUIBlockListBlockConfigurationElement } from './property-editor-ui-block-list-block-configuration.element.js';
import './property-editor-ui-block-list-block-configuration.element';

export default {
	title: 'Property Editor UIs/Block List Block Configuration',
	component: 'umb-property-editor-ui-block-list-block-configuration',
	id: 'umb-property-editor-ui-block-list-block-configuration',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUIBlockListBlockConfigurationElement> = () =>
	html`<umb-property-editor-ui-block-list-block-configuration></umb-property-editor-ui-block-list-block-configuration>`;
AAAOverview.storyName = 'Overview';
