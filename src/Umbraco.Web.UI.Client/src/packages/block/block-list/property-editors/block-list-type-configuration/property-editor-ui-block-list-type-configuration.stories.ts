import type { UmbPropertyEditorUIBlockListBlockConfigurationElement } from './property-editor-ui-block-list-type-configuration.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-block-list-type-configuration.element.js';

export default {
	title: 'Property Editor UIs/Block List Block Configuration',
	component: 'umb-property-editor-ui-block-list-type-configuration',
	id: 'umb-property-editor-ui-block-list-type-configuration',
} as Meta;

export const AAAOverview: StoryFn<UmbPropertyEditorUIBlockListBlockConfigurationElement> = () =>
	html`<umb-property-editor-ui-block-list-type-configuration></umb-property-editor-ui-block-list-type-configuration>`;
AAAOverview.storyName = 'Overview';
