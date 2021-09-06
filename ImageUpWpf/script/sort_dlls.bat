SET PWD=%CD%
ECHO Current at %PWD%
SET APPROOT=%PWD%\bin\Debug\netcoreapp3.1
CD %APPROOT%
ECHO Moving i18n DLLs...
(
	RMDIR /S /Q languages
	MKDIR languages
	MOVE /Y af-ZA languages
	MOVE /Y am-ET languages
	MOVE /Y ar-SA languages
	MOVE /Y az-Latn-AZ languages
	MOVE /Y be-BY languages
	MOVE /Y bg-BG languages
	MOVE /Y bn-BD languages
	MOVE /Y bs-Latn-BA languages
	MOVE /Y ca-ES languages
	MOVE /Y cs-CZ languages
	MOVE /Y da-DK languages
	MOVE /Y de-DE languages
	MOVE /Y el-GR languages
	MOVE /Y en-GB languages
	MOVE /Y es-ES languages
	MOVE /Y es-MX languages
	MOVE /Y et-EE languages
	MOVE /Y eu-ES languages
	MOVE /Y fa-IR languages
	MOVE /Y fi-FI languages
	MOVE /Y fr-CA languages
	MOVE /Y fr-FR languages
	MOVE /Y gl-ES languages
	MOVE /Y ha-Latn-NG languages
	MOVE /Y he-IL languages
	MOVE /Y hi-IN languages
	MOVE /Y hr-HR languages
	MOVE /Y hu-HU languages
	MOVE /Y id-ID languages
	MOVE /Y is-IS languages
	MOVE /Y it-IT languages
	MOVE /Y ja-JP languages
	MOVE /Y ka-GE languages
	MOVE /Y kk-KZ languages
	MOVE /Y km-KH languages
	MOVE /Y kn-IN languages
	MOVE /Y ko-KR languages
	MOVE /Y lo-LA languages
	MOVE /Y lt-LT languages
	MOVE /Y lv-LV languages
	MOVE /Y mk-MK languages
	MOVE /Y ml-IN languages
	MOVE /Y ms-MY languages
	MOVE /Y nb-NO languages
	MOVE /Y nl-NL languages
	MOVE /Y nn-NO languages
	MOVE /Y pl-PL languages
	MOVE /Y pt-BR languages
	MOVE /Y pt-PT languages
	MOVE /Y ro-RO languages
	MOVE /Y ru-RU languages
	MOVE /Y sk-SK languages
	MOVE /Y sl-SI languages
	MOVE /Y sq-AL languages
	MOVE /Y sr-Latn-RS languages
	MOVE /Y sv-SE languages
	MOVE /Y sw-KE languages
	MOVE /Y ta-IN languages
	MOVE /Y te-IN languages
	MOVE /Y th-TH languages
	MOVE /Y tr-TR languages
	MOVE /Y uk-UA languages
	MOVE /Y uz-Latn-UZ languages
	MOVE /Y vi-VN languages
	MOVE /Y zh-CN languages
	MOVE /Y zh-TW languages
) > NUL
echo Moved.
CD %APPROOT%