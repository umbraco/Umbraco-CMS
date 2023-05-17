import './data-type-workspace.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import { ifDefined } from 'lit/directives/if-defined.js';
import { data } from '../../../../shared/mocks/data/data-type.data';

import type { UmbDataTypeWorkspaceElement } from './data-type-workspace.element';

export default {
	title: 'Workspaces/Data Type',
	component: 'umb-data-type-workspace',
	id: 'umb-data-type-workspace',
} as Meta;

export const AAAOverview: Story<UmbDataTypeWorkspaceElement> = () =>
	html` <umb-data-type-workspace id="${ifDefined(data[0].id)}"></umb-data-type-workspace>`;
AAAOverview.storyName = 'Overview';
