import type { UmbPropertyEditorUICollectionColumnConfigurationElement } from './column-configuration.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './column-configuration.element.js';

export default {
	title: 'Property Editor UIs/Collection Column Configuration',
	component: 'umb-property-editor-ui-collection-column-configuration',
	id: 'umb-property-editor-ui-collection-column-configuration',
} as Meta;

export const AAAOverview: StoryFn<UmbPropertyEditorUICollectionColumnConfigurationElement> = () =>
	html`<umb-property-editor-ui-collection-column-configuration></umb-property-editor-ui-collection-column-configuration>`;
AAAOverview.storyName = 'Overview';
