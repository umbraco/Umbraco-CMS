import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbPropertyEditorUIImageCropperElement } from './property-editor-ui-image-cropper.element';
import './property-editor-ui-image-cropper.element';

export default {
	title: 'Property Editor UIs/Image Cropper',
	component: 'umb-property-editor-ui-image-cropper',
	id: 'umb-property-editor-ui-image-cropper',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUIImageCropperElement> = () =>
	html`<umb-property-editor-ui-image-cropper></umb-property-editor-ui-image-cropper>`;
AAAOverview.storyName = 'Overview';
