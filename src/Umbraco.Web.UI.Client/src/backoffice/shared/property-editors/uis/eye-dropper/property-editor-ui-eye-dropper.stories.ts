import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbPropertyEditorUIEyeDropperElement } from './property-editor-ui-eye-dropper.element';
import './property-editor-ui-eye-dropper.element';

export default {
	title: 'Property Editor UIs/Eye Dropper',
	component: 'umb-property-editor-ui-eye-dropper',
	id: 'umb-property-editor-ui-eye-dropper',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUIEyeDropperElement> = () =>
	html`<umb-property-editor-ui-eye-dropper></umb-property-editor-ui-eye-dropper>`;
AAAOverview.storyName = 'Overview';
