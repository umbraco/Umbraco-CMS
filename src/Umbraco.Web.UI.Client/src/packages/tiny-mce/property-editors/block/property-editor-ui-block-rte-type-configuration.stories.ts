import type { UmbPropertyEditorUIBlockRteBlockConfigurationElement } from './property-editor-ui-block-rte-type-configuration.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-block-rte-type-configuration.element.js';

export default {
	title: 'Property Editor UIs/Block Rte Block Configuration',
	component: 'umb-property-editor-ui-block-rte-type-configuration',
	id: 'umb-property-editor-ui-block-rte-type-configuration',
} as Meta;

export const AAAOverview: StoryFn<UmbPropertyEditorUIBlockRteBlockConfigurationElement> = () =>
	html`<umb-property-editor-ui-block-rte-type-configuration></umb-property-editor-ui-block-rte-type-configuration>`;
AAAOverview.storyName = 'Overview';
