import './dictionary-workspace.element';
import { Meta, Story } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';
import { ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { data } from '../../../../mocks/data/dictionary.data.js';
import type { UmbWorkspaceDictionaryElement } from './dictionary-workspace.element.js';

export default {
	title: 'Workspaces/Dictionary',
	component: 'umb-dictionary-workspace',
	id: 'umb-dictionary-workspace',
} as Meta;

export const AAAOverview: Story<UmbWorkspaceDictionaryElement> = () =>
	html` <umb-dictionary-workspace id="${ifDefined(data[0].id)}"></umb-dictionary-workspace>`;

AAAOverview.storyName = 'Overview';
