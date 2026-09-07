import type { UmbLocalizationDictionary } from '@umbraco-cms/backoffice/localization-api';

export default {
  searchExamine: {
    showFields: 'Vis felter',
    headline: 'Søgedokumentfelter',
    filterPlaceholder: 'Filtrer felter efter navn eller v\u00E6rdi...',
    filterLabel: 'Filtrer felter efter navn eller v\u00E6rdi',
    fieldCount: (count: number) => {
      switch (count) {
        case 1:
          return '1 felt';
        default:
          return `${count} felter`;
      }
    },
    tableColumnName: 'Navn',
    tableColumnValue: 'V\u00E6rdi',
    copyValue: 'Kopi\u00E9r v\u00E6rdi',
    seeMore: 'Se mere',
    seeLess: 'Se mindre',
    noFieldsMatch: 'Ingen felter matcher dit filter.',
    noFields: 'Dette dokument har ingen indekserede felter.',
    loadError: 'Kunne ikke indl\u00E6se dokumentfelter. Pr\u00F8v venligst igen.',
    valueIndex: (index: number) => `V\u00E6rdi ${index}`,
    fieldType: (type: string) => {
      switch (type) {
        case 'keywords':
          return 'N\u00F8gleord (eksakt match)';
        case 'texts':
          return 'Fuldtekst';
        case 'textsr1':
          return 'Fuldtekst (Boost: H\u00F8j)';
        case 'textsr2':
          return 'Fuldtekst (Boost: Medium)';
        case 'textsr3':
          return 'Fuldtekst (Boost: Lav)';
        case 'integers':
          return 'Heltal';
        case 'decimals':
          return 'Decimal';
        case 'datetimeoffsets':
          return 'Dato/tid';
        default:
          return type;
      }
    },
  },
} satisfies UmbLocalizationDictionary;
