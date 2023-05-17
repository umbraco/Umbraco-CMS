import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbPropertyEditorUIImageCropsConfigurationElement } from './property-editor-ui-image-crops-configuration.element';
import './property-editor-ui-image-crops-configuration.element';

export default {
	title: 'Property Editor UIs/Image Crops Configuration',
	component: 'umb-property-editor-ui-image-crops-configuration',
	id: 'umb-property-editor-ui-image-crops-configuration',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUIImageCropsConfigurationElement> = () =>
	html`<umb-property-editor-ui-image-crops-configuration></umb-property-editor-ui-image-crops-configuration>`;
AAAOverview.storyName = 'Overview';
