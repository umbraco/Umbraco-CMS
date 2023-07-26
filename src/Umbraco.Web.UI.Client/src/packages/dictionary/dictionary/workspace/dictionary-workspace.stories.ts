import './dictionary-workspace.element.js';
import { Meta, Story } from '@storybook/web-components';
import { data } from '../../../../mocks/data/dictionary.data.js';
import type { UmbWorkspaceDictionaryElement } from './dictionary-workspace.element.js';
import { html, ifDefined } from '@umbraco-cms/backoffice/external/lit';

export default {
	title: 'Workspaces/Dictionary',
	component: 'umb-dictionary-workspace',
	id: 'umb-dictionary-workspace',
} as Meta;

export const AAAOverview: Story<UmbWorkspaceDictionaryElement> = () =>
	html` <umb-dictionary-workspace id="${ifDefined(data[0].id)}"></umb-dictionary-workspace>`;

AAAOverview.storyName = 'Overview';
