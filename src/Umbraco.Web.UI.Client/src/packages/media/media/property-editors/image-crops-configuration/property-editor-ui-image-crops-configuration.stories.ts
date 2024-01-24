import { Meta, Story } from '@storybook/web-components';
import type { UmbPropertyEditorUIImageCropsConfigurationElement } from './property-editor-ui-image-crops-configuration.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-image-crops-configuration.element.js';

export default {
	title: 'Property Editor UIs/Image Crops Configuration',
	component: 'umb-property-editor-ui-image-crops-configuration',
	id: 'umb-property-editor-ui-image-crops-configuration',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUIImageCropsConfigurationElement> = () =>
	html`<umb-property-editor-ui-image-crops-configuration></umb-property-editor-ui-image-crops-configuration>`;
AAAOverview.storyName = 'Overview';
