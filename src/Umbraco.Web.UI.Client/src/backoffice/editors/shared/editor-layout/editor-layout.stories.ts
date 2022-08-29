import './editor-layout.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbEditorLayout } from './editor-layout.element';

export default {
	title: 'Editors/Shared/Editor Layout',
	component: 'umb-editor-layout',
	id: 'umb-editor-layout',
} as Meta;

export const AAAOverview: Story<UmbEditorLayout> = () => html` <umb-editor-layout>
	<div slot="header"><uui-button color="" look="placeholder">Header slot</uui-button></div>
	<uui-button color="" look="placeholder">Main slot</uui-button>
	<div slot="footer"><uui-button color="" look="placeholder">Footer slot</uui-button></div>
</umb-editor-layout>`;
AAAOverview.storyName = 'Overview';
