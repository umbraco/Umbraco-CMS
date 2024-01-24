import { Meta, Story } from '@storybook/web-components';
import type { UmbPropertyEditorUIBlockListBlockConfigurationElement } from './property-editor-ui-block-list-type-configuration.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-block-list-type-configuration.element.js';

export default {
	title: 'Property Editor UIs/Block List Block Configuration',
	component: 'umb-property-editor-ui-block-list-type-configuration',
	id: 'umb-property-editor-ui-block-list-type-configuration',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUIBlockListBlockConfigurationElement> = () =>
	html`<umb-property-editor-ui-block-list-type-configuration></umb-property-editor-ui-block-list-type-configuration>`;
AAAOverview.storyName = 'Overview';
