import { Meta, StoryObj } from '@storybook/web-components';
import './input-checkbox-list.element';
import type { UmbInputCheckboxListElement } from './input-checkbox-list.element';

const meta: Meta<UmbInputCheckboxListElement> = {
    title: 'Components/Inputs/Checkbox List',
    component: 'umb-input-checkbox-list'
};
  
export default meta;
type Story = StoryObj<UmbInputCheckboxListElement>;
  
export const Overview: Story = {
    args: {
        list: [
            {
                key: "isAwesome",
                value: "Umbraco is awesome?",
                checked: true
            },
            {
                key: "attendingCodeGarden",
                value: "Attending CodeGarden?",
                checked: false
            },
        ]
    }
};