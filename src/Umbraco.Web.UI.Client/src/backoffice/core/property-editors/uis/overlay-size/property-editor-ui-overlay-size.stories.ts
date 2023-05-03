import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbPropertyEditorUIOverlaySizeElement } from './property-editor-ui-overlay-size.element';
import './property-editor-ui-overlay-size.element';

export default {
	title: 'Property Editor UIs/Overlay Size',
	component: 'umb-property-editor-ui-overlay-size',
	id: 'umb-property-editor-ui-overlay-size',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUIOverlaySizeElement> = () =>
	html`<umb-property-editor-ui-overlay-size></umb-property-editor-ui-overlay-size>`;
AAAOverview.storyName = 'Overview';
