import { Meta, StoryObj } from '@storybook/web-components';
import './input-section.element';
import type { UmbInputPickerSectionElement } from './input-section.element';

const meta: Meta<UmbInputPickerSectionElement> = {
    title: 'Components/Inputs/Section',
    component: 'umb-input-section',
    argTypes: {
        modalType: {
            control: 'inline-radio',
            options: ['dialog', 'sidebar'],
            defaultValue: 'sidebar',
            description: 'The type of modal to use when selecting sections',
        },
        modalSize:{
            control: 'select',
            options: ['small', 'medium', 'large', 'full'],
            defaultValue: 'small',
            description: 'The size of the modal to use when selecting sections, only applicable to sidebar not dialog',
        }
    }
};
  
export default meta;
type Story = StoryObj<UmbInputPickerSectionElement>;
  
export const Overview: Story = {
    args: {
        modalType: 'sidebar',
    }
};

export const WithDialog: Story = {
    args: {
        modalType: 'dialog'
    }
};

export const WithFullSidebar: Story = {
    args: {
        modalType: 'sidebar',
        modalSize: 'full'
    }
};