import './dictionary-workspace.element';
import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';
import { data } from '../../../../core/mocks/data/dictionary.data';
import type { UmbWorkspaceDictionaryElement } from './dictionary-workspace.element';

export default {
	title: 'Workspaces/Dictionary',
	component: 'umb-dictionary-workspace',
	id: 'umb-dictionary-workspace',
} as Meta;

export const AAAOverview: Story<UmbWorkspaceDictionaryElement> = () =>
	html` <umb-dictionary-workspace id="${data[0].key}"></umb-dictionary-workspace>`;

AAAOverview.storyName = 'Overview';
