import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbEditorPropertyLayoutElement } from './editor-property-layout.element';
import './editor-property-layout.element';

export default {
	title: 'Editors/Shared/Editor Property Layout',
	component: 'umb-editor-property-layout',
	id: 'umb-editor-property-layout',
} as Meta;

export const AAAOverview: Story<UmbEditorPropertyLayoutElement> = () => html` <umb-editor-property-layout
	label="Label"
	description="Description">
	<div slot="property-action-menu"><uui-button color="" look="placeholder">Menu</uui-button></div>

	<div slot="editor"><uui-button color="" look="placeholder">Editor</uui-button></div>
</umb-editor-property-layout>`;
AAAOverview.storyName = 'Overview';
