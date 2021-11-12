#pragma once
#include <map>

class EventDataUtil
{
public:
	static EventDataUtil& Get()
	{
		static EventDataUtil instance;
		return instance;
	}

	void AddFileObjectToPath(std::wstring& fileObject, std::wstring& path)
	{
		auto entry = m_FileObjectToPath.find(fileObject);
		if (entry == m_FileObjectToPath.end())
			m_FileObjectToPath.insert(std::pair<std::wstring, std::wstring>(fileObject, path));
	}

	const std::wstring *GetPathFromFileObject(std::wstring& fileObject)
	{
		auto entry = m_FileObjectToPath.find(fileObject);
		if (entry != m_FileObjectToPath.end())
		{
			return &entry->second;
		}

		return nullptr;
	}

	void Clear()
	{
		m_FileObjectToPath.clear();
	}

private:
	std::map<std::wstring, std::wstring> m_FileObjectToPath;
};

