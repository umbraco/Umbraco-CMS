import type { UmbPropertyEditorUICollectionLayoutConfigurationElement } from './layout-configuration.element.js';
import type { Meta, Story } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './layout-configuration.element.js';

export default {
	title: 'Property Editor UIs/Collection Layout Configuration',
	component: 'umb-property-editor-ui-collection-layout-configuration',
	id: 'umb-property-editor-ui-collection-layout-configuration',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUICollectionLayoutConfigurationElement> = () =>
	html`<umb-property-editor-ui-collection-layout-configuration></umb-property-editor-ui-collection-layout-configuration>`;
AAAOverview.storyName = 'Overview';
