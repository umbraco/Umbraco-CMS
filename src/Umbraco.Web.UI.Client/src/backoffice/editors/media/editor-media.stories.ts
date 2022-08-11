import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import { UmbEditorMediaElement } from './editor-media.element';
import './editor-media.element';

import { data } from '../../../mocks/data/node.data';

export default {
	title: 'Editors/Media',
	component: 'umb-editor-media',
	id: 'umb-editor-media',
} as Meta;

const mediaNodes = data.filter((node) => node.type === 'media');

export const AAAOverview: Story<UmbEditorMediaElement> = () =>
	html` <umb-editor-media id="${mediaNodes[0].id}"></umb-editor-media>`;
AAAOverview.storyName = 'Overview';
