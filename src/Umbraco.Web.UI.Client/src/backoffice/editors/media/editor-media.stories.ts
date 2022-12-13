import './editor-media.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import { data as mediaNodes } from '../../../core/mocks/data/media.data';

import type { UmbEditorMediaElement } from './editor-media.element';

export default {
	title: 'Editors/Media',
	component: 'umb-editor-media',
	id: 'umb-editor-media',
} as Meta;

export const AAAOverview: Story<UmbEditorMediaElement> = () =>
	html` <umb-editor-media id="${mediaNodes[0].key}"></umb-editor-media>`;
AAAOverview.storyName = 'Overview';
